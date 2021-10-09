namespace Pico8Emulator.Backend {
	public abstract class InputBackend {
		public Emulator Emulator;
		public abstract bool IsButtonDown(int i, int p);
	}
}