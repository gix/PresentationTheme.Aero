namespace StyleCore.Native
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct SIZE
    {
        public int cx;
        public int cy;

        public override string ToString()
        {
            return string.Concat("cx=", cx, ", cy=", cy);
        }
    }
}
