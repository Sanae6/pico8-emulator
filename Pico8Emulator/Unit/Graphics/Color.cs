namespace Pico8Emulator.Unit.Graphics {
    public struct Color {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public static Color FromRgba(byte r, byte g, byte b, byte a = 255) {
            return new Color {
                R = r,
                G = g,
                B = b,
                A = a,
            };
        }
    }
}