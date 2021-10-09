using System;
using Pico8Emulator.Unit.Graphics;

namespace Pico8Emulator.Backend {
    public abstract class Image : IDisposable {
        protected Image(string filename) {}
        protected Image(int width, int height) {}
        public abstract Color this[int x, int y] { get; set; }
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract void Save(string location);
        public virtual void Dispose() {}
    }
}