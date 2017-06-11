namespace ThemeCore.Native
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public class UxColorScheme
    {
        public uint ActiveTitle;
        public uint Background;
        public uint ButtonFace;
        public uint ButtonText;
        public uint GrayText;
        public uint Hilight;
        public uint HilightText;
        public uint HotTrackingColor;
        public uint InactiveTitle;
        public uint InactiveTitleText;
        public uint TitleText;
        public uint Window;
        public uint WindowText;
        public uint Scrollbar;
        public uint Menu;
        public uint WindowFrame;
        public uint MenuText;
        public uint ActiveBorder;
        public uint InactiveBorder;
        public uint AppWorkspace;
        public uint ButtonShadow;
        public uint ButtonHilight;
        public uint ButtonDkShadow;
        public uint ButtonLight;
        public uint InfoText;
        public uint InfoWindow;
        public uint ButtonAlternateFace;
        public uint GradientActiveTitle;
        public uint GradientInactiveTitle;
        public uint MenuHilight;
        public uint MenuBar;
    }
}
