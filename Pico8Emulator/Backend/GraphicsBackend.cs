namespace Pico8Emulator.Backend {
	public abstract class GraphicsBackend {
		public Emulator Emulator;

		public abstract void CreateSurface();
		public abstract void Flip();
		public abstract void Draw();
	}
}
