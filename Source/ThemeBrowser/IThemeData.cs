namespace ThemeBrowser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using StyleCore;
    using StyleCore.Native;
    using Color = System.Windows.Media.Color;

    public interface IThemeData : IDisposable
    {
        bool IsValid { get; }
        bool? GetThemeBool(int partId, int stateId, int propertyId);
        Color? GetThemeColor(int partId, int stateId, int propertyId);
        int? GetThemeEnumValue(int partId, int stateId, int propertyId);
        string GetThemeFilename(int partId, int stateId, int propertyId);
        LOGFONT GetThemeFont(int partId, int stateId, int propertyId);
        IntPtr? GetThemeBitmap(int partId, int stateId, int propertyId);
        int? GetThemeInt(int partId, int stateId, int propertyId);
        int? GetThemeMetric(int partId, int stateId, int propertyId);
        INTLIST GetThemeIntList(int partId, int stateId, int propertyId);
        MARGINS? GetThemeMargins(int partId, int stateId, int propertyId);
        POINT? GetThemePosition(int partId, int stateId, int propertyId);
        RECT? GetThemeRect(int partId, int stateId, int propertyId);
        SIZE? GetThemePartSize(int partId, int stateId, ThemeSize themeSize);
        Stream GetThemeStream(int partId, int stateId, int propertyId, SafeModuleHandle instance);
        string GetThemeString(int partId, int stateId, int propertyId);
        HResult GetThemeTransitionDuration(int partId, int stateFrom, int stateTo, int propertyId, out uint duration);
        PropertyOrigin GetThemePropertyOrigin(int partId, int stateId, int propertyId);
        bool GetThemeSysBool(int propertyId);
        Color GetThemeSysColor(int propertyId);
        LOGFONT GetThemeSysFont(int propertyId);
        int? GetThemeSysInt(int propertyId);
        string GetThemeSysString(int propertyId);
    }
}