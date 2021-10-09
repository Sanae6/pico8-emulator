﻿using Pico8Emulator.Backend;
using Pico8Emulator.lua;
using Pico8Emulator.Unit;
using Pico8Emulator.Unit.Audio;
using Pico8Emulator.Unit.Cart;
using Pico8Emulator.Unit.Input;
using Pico8Emulator.Unit.Math;
using Pico8Emulator.Unit.Mem;
using System;
using System.Threading;
using System.Collections.Generic;
using GraphicsUnit = Pico8Emulator.Unit.Graphics.GraphicsUnit;
using System.Diagnostics;

namespace Pico8Emulator {
	public class Emulator {
		public readonly List<Unit.Unit> units = new();

		public MemoryUnit Memory;
		public GraphicsUnit Graphics;
		public AudioUnit Audio;
		public MathUnit Math;
		public InputUnit Input;
		public CartridgeUnit CartridgeLoader;

		public readonly GraphicsBackend GraphicsBackend;
		public readonly AudioBackend AudioBackend;
		public readonly InputBackend InputBackend;
		public readonly Type ImageBackend;

		public Thread gameThread;
		public Stopwatch cartLoopTimer = new Stopwatch();

		public double UpdateTime;

		public long GameLoopCalls { get; private set; } = 0;

		private bool exitGame = false;

		public Emulator(GraphicsBackend graphics, AudioBackend audio, InputBackend input, Type imageType) {
			GraphicsBackend = graphics;
			AudioBackend = audio;
			InputBackend = input;
			ImageBackend = imageType;

			graphics.Emulator = this;
			audio.Emulator = this;
			input.Emulator = this;

			units.Add(Memory = new MemoryUnit(this));
			units.Add(Graphics = new GraphicsUnit(this));
			units.Add(Audio = new AudioUnit(this));
			units.Add(Math = new MathUnit(this));
			units.Add(Input = new InputUnit(this));
			units.Add(CartridgeLoader = new CartridgeUnit(this));

			foreach (var unit in units) {
				unit.Init();
			}
		}

		public void TriggerExitGame()
		{
			exitGame = true;
		}

		public void WaitForGameToFinish()
		{
			gameThread?.Join();
		}

		public bool LoadCartridge(string path)
		{
			TriggerExitGame();
			WaitForGameToFinish();

			return CartridgeLoader.Load(path);
		}

		public void Run()
		{
			TriggerExitGame();

			gameThread = new Thread(() => {
				CartridgeLoader.Run();
			}) {
				IsBackground = false,
				Priority = ThreadPriority.Highest
			};

			gameThread.Start();
		}

		public void GameLoop()
		{
			exitGame = false;

			Action update;
			if (CartridgeLoader.HighFps)
			{
				UpdateTime = 1f / 60f * 1000;
				update = Update60;
			}
			else
			{
				UpdateTime = 1f / 30f * 1000;
				update = Update30;
			}

			cartLoopTimer.Restart();
			while (true)
			{
				if (exitGame)
				{
					break;
				}

				GameLoopCalls++;

				update();
				Draw();

				//
				// After draw(), wait for the correct amount of time depending on the
				// set FPS.
				//

				GraphicsBackend.Flip();

				cartLoopTimer.Stop();
				var sleepTime = (int)(UpdateTime - cartLoopTimer.ElapsedMilliseconds);
				Thread.Sleep(sleepTime < 0 ? 0 : sleepTime);
				cartLoopTimer.Restart();
			}
		}

		public void Shutdown() {
			foreach (var unit in units) {
				unit.Destroy();
			}
		}

		public void Update30() {
			foreach (var unit in units)
			{
				unit.Update();
			}

			CartridgeLoader.Update30();
		}

		public void Update60() {
			foreach (var unit in units) {
				unit.Update();
			}

			CartridgeLoader.Update60();
		}

		public void Draw() {
			CartridgeLoader.Draw();
		}

		public void InitApi(LuaInterpreter script) {
			foreach (var unit in units) {
				unit.DefineApi(script);
			}

			script.AddFunction("printh", (Action<object>)Printh);
			script.AddFunction("stat", (Func<object>)Stat);
			script.AddFunction("menuitem", (Action<int, string, object>)Menuitem);

			script.RunScript(LuaPatcher.PatchCode(Api.All));
		}

		public void Menuitem(int index, string label = null, object callback = null) {
			// TODO: implement
		}

		public void Printh(object s) {
			Console.WriteLine($"{s:####.####}");
		}

		public object Stat() {
			return 0; // TODO: implement
		} 
	}
}