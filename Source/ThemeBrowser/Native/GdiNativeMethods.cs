namespace ThemeBrowser.Native
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using ThemeCore.Native;

    public static class GdiNativeMethods
    {
        [DllImport("gdi32")]
        public static extern bool GdiDrawStream(
            IntPtr hDC, uint dwStructSize, ref GDIDRAWSTREAM pStream);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern int SetBkMode(IntPtr hdc, int iBkMode);

        public static SafeBitmapHandle GetHbitmapHandle(this Bitmap bitmap)
        {
            return new SafeBitmapHandle(bitmap.GetHbitmap(), true);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GDIDRAWSTREAM
    {
        public uint signature;      // = 0x44727753;//"Swrd"
        public uint reserved;       // Zero value.
        public uint hDC;            // handle to the device object of window to draw.
        public RECT rcDest;         // desination rect of window to draw.
        public uint one;            // must be 1.
        public uint hImage;         // handle to the specia bitmap image.
        public uint nine;           // must be 9.
        public RECT rcClip;         // desination rect of window to draw.
        public RECT rcSrc;          // source rect of bitmap to draw.
        public uint drawOption;     // option flag for drawing image.
        public uint leftArcValue;   // arc value of left side.
        public uint rightArcValue;  // arc value of right side.
        public uint topArcValue;    // arc value of top side.
        public uint bottomArcValue; // arc value of bottom side.
        public uint crTransparent;  // transparent color.
    }
}
