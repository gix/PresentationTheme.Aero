namespace ThemeCore.Native
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;

        public override string ToString()
        {
            return string.Concat(
                "Left=", cxLeftWidth, ", Right=", cxRightWidth,
                ", Top=", cyTopHeight, ", Bottom=", cyBottomHeight);
        }
    }
}
