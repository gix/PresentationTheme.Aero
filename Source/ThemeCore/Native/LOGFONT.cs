namespace ThemeCore.Native
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class LOGFONT
    {
        public int lfHeight;
        public int lfWidth;
        public int lfEscapement;
        public int lfOrientation;
        public int lfWeight;
        public byte lfItalic;
        public byte lfUnderline;
        public byte lfStrikeOut;
        public byte lfCharSet;
        public byte lfOutPrecision;
        public byte lfClipPrecision;
        public byte lfQuality;
        public byte lfPitchAndFamily;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
        public string lfFaceName;

        public LOGFONT()
        {
        }

        public LOGFONT(LOGFONT lf)
        {
            lfHeight = lf.lfHeight;
            lfWidth = lf.lfWidth;
            lfEscapement = lf.lfEscapement;
            lfOrientation = lf.lfOrientation;
            lfWeight = lf.lfWeight;
            lfItalic = lf.lfItalic;
            lfUnderline = lf.lfUnderline;
            lfStrikeOut = lf.lfStrikeOut;
            lfCharSet = lf.lfCharSet;
            lfOutPrecision = lf.lfOutPrecision;
            lfClipPrecision = lf.lfClipPrecision;
            lfQuality = lf.lfQuality;
            lfPitchAndFamily = lf.lfPitchAndFamily;
            lfFaceName = lf.lfFaceName;
        }
    }
}
