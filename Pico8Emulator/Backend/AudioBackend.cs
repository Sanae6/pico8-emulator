namespace Pico8Emulator.Backend {
	public abstract class AudioBackend {
		public Emulator Emulator;

		public abstract void Update();
	}
}