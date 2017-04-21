namespace StyleCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class ThemeInfo
    {
        private static readonly Tuple<int, string>[] EmptyArray = new Tuple<int, string>[0];
        private static readonly ClassCache EmptyClass = new ClassCache();
        private static readonly IEqualityComparer<string> Comparer = StringComparer.OrdinalIgnoreCase;
        private static readonly Dictionary<string, ClassCache> PartMap =
            new Dictionary<string, ClassCache>(Comparer);

        private class ClassCache
        {
            private readonly Dictionary<int, Tuple<int, string>[]> statesMap =
                new Dictionary<int, Tuple<int, string>[]>();

            public ClassCache()
            {
                Parts = EmptyArray;
            }

            public ClassCache(TypeInfo enumType)
            {
                Parts = Entries(enumType);

                foreach (var value in Enum.GetValues(enumType)) {
                    var statesAttrib = GetAttributeOfType<VisualStatesAttribute>(value);
                    if (statesAttrib != null)
                        statesMap[(int)value] = Entries(statesAttrib.StatesEnumType);
                }
            }

            public Tuple<int, string>[] Parts { get; }

            public Tuple<int, string>[] GetStates(int partId)
            {
                Tuple<int, string>[] entries;
                if (!statesMap.TryGetValue(partId, out entries))
                    statesMap[partId] = entries = EmptyArray;
                return entries;
            }

            private static T GetAttributeOfType<T>(object enumVal) where T : Attribute
            {
                var type = enumVal.GetType();
                var memInfo = type.GetMember(enumVal.ToString());
                var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
                return attributes.Length > 0 ? (T)attributes[0] : null;
            }
        }

        static ThemeInfo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var definedType in assembly.DefinedTypes) {
                if (!definedType.IsEnum)
                    continue;

                var attribs = definedType.GetCustomAttributes<VisualClassAttribute>().ToList();
                if (attribs.Count == 0)
                    continue;

                var @class = new ClassCache(definedType);
                foreach (var attrib in attribs)
                    PartMap[attrib.ClassName] = @class;
            }
        }

        private static ClassCache GetClass(string className)
        {
            ClassCache entry;
            if (!PartMap.TryGetValue(className, out entry))
                PartMap[className] = entry = EmptyClass;
            return entry;
        }

        public static Tuple<int, string>[] GetParts(string className)
        {
            return GetClass(className).Parts;
        }

        public static Tuple<int, string>[] GetStates(ThemePart part)
        {
            return GetClass(part.Parent.Name).GetStates(part.Id);
        }

        public static string GetPartName(string className, int partId)
        {
            return GetClass(className).Parts.FirstOrDefault(x => x.Item1 == partId)?.Item2;
        }

        public static string GetStateName(string className, int partId, int stateId)
        {
            return GetClass(className).GetStates(partId).FirstOrDefault(x => x.Item1 == stateId)?.Item2;
        }

        public static ThemePropertyType GetPropertyType(TMT property)
        {
            switch (property) {
                case TMT.DIBDATA:
                    return ThemePropertyType.HBitmap;
                case TMT.GLYPHDIBDATA:
                    return ThemePropertyType.HBitmap;

                case TMT.ALWAYSSHOWSIZINGBAR:
                    return ThemePropertyType.Bool;
                case TMT.AUTOSIZE:
                    return ThemePropertyType.Bool;
                case TMT.BGFILL:
                    return ThemePropertyType.Bool;
                case TMT.BORDERONLY:
                    return ThemePropertyType.Bool;
                case TMT.COMPOSITED:
                    return ThemePropertyType.Bool;
                case TMT.COMPOSITEDOPAQUE:
                    return ThemePropertyType.Bool;
                case TMT.DRAWBORDERS:
                    return ThemePropertyType.Bool;
                case TMT.FLATMENUS:
                    return ThemePropertyType.Bool;
                case TMT.GLYPHONLY:
                    return ThemePropertyType.Bool;
                case TMT.GLYPHTRANSPARENT:
                    return ThemePropertyType.Bool;
                case TMT.INTEGRALSIZING:
                    return ThemePropertyType.Bool;
                case TMT.LOCALIZEDMIRRORIMAGE:
                    return ThemePropertyType.Bool;
                case TMT.MIRRORIMAGE:
                    return ThemePropertyType.Bool;
                case TMT.NOETCHEDEFFECT:
                    return ThemePropertyType.Bool;
                case TMT.SCALEDBACKGROUND:
                    return ThemePropertyType.Bool;
                case TMT.SOURCEGROW:
                    return ThemePropertyType.Bool;
                case TMT.SOURCESHRINK:
                    return ThemePropertyType.Bool;
                case TMT.TEXTAPPLYOVERLAY:
                    return ThemePropertyType.Bool;
                case TMT.TEXTGLOW:
                    return ThemePropertyType.Bool;
                case TMT.TEXTITALIC:
                    return ThemePropertyType.Bool;
                case TMT.TRANSPARENT:
                    return ThemePropertyType.Bool;
                case TMT.UNIFORMSIZING:
                    return ThemePropertyType.Bool;
                case TMT.USERPICTURE:
                    return ThemePropertyType.Bool;

                case TMT.ACCENTCOLORHINT:
                    return ThemePropertyType.Color;
                case TMT.ACTIVEBORDER:
                    return ThemePropertyType.Color;
                case TMT.ACTIVECAPTION:
                    return ThemePropertyType.Color;
                case TMT.APPWORKSPACE:
                    return ThemePropertyType.Color;
                case TMT.BACKGROUND:
                    return ThemePropertyType.Color;
                case TMT.BLENDCOLOR:
                    return ThemePropertyType.Color;
                case TMT.BODYTEXTCOLOR:
                    return ThemePropertyType.Color;
                case TMT.BORDERCOLOR:
                    return ThemePropertyType.Color;
                case TMT.BORDERCOLORHINT:
                    return ThemePropertyType.Color;
                case TMT.BTNFACE:
                    return ThemePropertyType.Color;
                case TMT.BTNHIGHLIGHT:
                    return ThemePropertyType.Color;
                case TMT.BTNSHADOW:
                    return ThemePropertyType.Color;
                case TMT.BTNTEXT:
                    return ThemePropertyType.Color;
                case TMT.BUTTONALTERNATEFACE:
                    return ThemePropertyType.Color;
                case TMT.CAPTIONTEXT:
                    return ThemePropertyType.Color;
                case TMT.DKSHADOW3D:
                    return ThemePropertyType.Color;
                case TMT.EDGEDKSHADOWCOLOR:
                    return ThemePropertyType.Color;
                case TMT.EDGEFILLCOLOR:
                    return ThemePropertyType.Color;
                case TMT.EDGEHIGHLIGHTCOLOR:
                    return ThemePropertyType.Color;
                case TMT.EDGELIGHTCOLOR:
                    return ThemePropertyType.Color;
                case TMT.EDGESHADOWCOLOR:
                    return ThemePropertyType.Color;
                case TMT.FILLCOLOR:
                    return ThemePropertyType.Color;
                case TMT.FILLCOLORHINT:
                    return ThemePropertyType.Color;
                case TMT.FROMCOLOR1:
                    return ThemePropertyType.Color;
                case TMT.FROMCOLOR2:
                    return ThemePropertyType.Color;
                case TMT.FROMCOLOR3:
                    return ThemePropertyType.Color;
                case TMT.FROMCOLOR4:
                    return ThemePropertyType.Color;
                case TMT.FROMCOLOR5:
                    return ThemePropertyType.Color;
                case TMT.GLOWCOLOR:
                    return ThemePropertyType.Color;
                case TMT.GLYPHTEXTCOLOR:
                    return ThemePropertyType.Color;
                case TMT.GLYPHTRANSPARENTCOLOR:
                    return ThemePropertyType.Color;
                case TMT.GRADIENTACTIVECAPTION:
                    return ThemePropertyType.Color;
                case TMT.GRADIENTCOLOR1:
                    return ThemePropertyType.Color;
                case TMT.GRADIENTCOLOR2:
                    return ThemePropertyType.Color;
                case TMT.GRADIENTCOLOR3:
                    return ThemePropertyType.Color;
                case TMT.GRADIENTCOLOR4:
                    return ThemePropertyType.Color;
                case TMT.GRADIENTCOLOR5:
                    return ThemePropertyType.Color;
                case TMT.GRADIENTINACTIVECAPTION:
                    return ThemePropertyType.Color;
                case TMT.GRAYTEXT:
                    return ThemePropertyType.Color;
                case TMT.HEADING1TEXTCOLOR:
                    return ThemePropertyType.Color;
                case TMT.HEADING2TEXTCOLOR:
                    return ThemePropertyType.Color;
                case TMT.HIGHLIGHT:
                    return ThemePropertyType.Color;
                case TMT.HIGHLIGHTTEXT:
                    return ThemePropertyType.Color;
                case TMT.HOTTRACKING:
                    return ThemePropertyType.Color;
                case TMT.INACTIVEBORDER:
                    return ThemePropertyType.Color;
                case TMT.INACTIVECAPTION:
                    return ThemePropertyType.Color;
                case TMT.INACTIVECAPTIONTEXT:
                    return ThemePropertyType.Color;
                case TMT.INFOBK:
                    return ThemePropertyType.Color;
                case TMT.INFOTEXT:
                    return ThemePropertyType.Color;
                case TMT.LIGHT3D:
                    return ThemePropertyType.Color;
                case TMT.MENU:
                    return ThemePropertyType.Color;
                case TMT.MENUBAR:
                    return ThemePropertyType.Color;
                case TMT.MENUHILIGHT:
                    return ThemePropertyType.Color;
                case TMT.MENUTEXT:
                    return ThemePropertyType.Color;
                case TMT.SCROLLBAR:
                    return ThemePropertyType.Color;
                case TMT.SHADOWCOLOR:
                    return ThemePropertyType.Color;
                case TMT.TEXTBORDERCOLOR:
                    return ThemePropertyType.Color;
                case TMT.TEXTCOLOR:
                    return ThemePropertyType.Color;
                case TMT.TEXTCOLORHINT:
                    return ThemePropertyType.Color;
                case TMT.TEXTSHADOWCOLOR:
                    return ThemePropertyType.Color;
                case TMT.TRANSPARENTCOLOR:
                    return ThemePropertyType.Color;
                case TMT.WINDOW:
                    return ThemePropertyType.Color;
                case TMT.WINDOWFRAME:
                    return ThemePropertyType.Color;
                case TMT.WINDOWTEXT:
                    return ThemePropertyType.Color;

                case TMT.ATLASIMAGE:
                    return ThemePropertyType.DiskStream;

                case TMT.BGTYPE:
                    return ThemePropertyType.Enum;
                case TMT.BORDERTYPE:
                    return ThemePropertyType.Enum;
                case TMT.CONTENTALIGNMENT:
                    return ThemePropertyType.Enum;
                case TMT.FILLTYPE:
                    return ThemePropertyType.Enum;
                case TMT.GLYPHTYPE:
                    return ThemePropertyType.Enum;
                case TMT.GLYPHFONTSIZINGTYPE:
                    return ThemePropertyType.Enum;
                case TMT.HALIGN:
                    return ThemePropertyType.Enum;
                case TMT.ICONEFFECT:
                    return ThemePropertyType.Enum;
                case TMT.IMAGELAYOUT:
                    return ThemePropertyType.Enum;
                case TMT.IMAGESELECTTYPE:
                    return ThemePropertyType.Enum;
                case TMT.OFFSETTYPE:
                    return ThemePropertyType.Enum;
                case TMT.SIZINGTYPE:
                    return ThemePropertyType.Enum;
                case TMT.TEXTSHADOWTYPE:
                    return ThemePropertyType.Enum;
                case TMT.TRUESIZESCALINGTYPE:
                    return ThemePropertyType.Enum;
                case TMT.VALIGN:
                    return ThemePropertyType.Enum;

                case TMT.GLYPHIMAGEFILE:
                    return ThemePropertyType.Filename;
                case TMT.IMAGEFILE:
                    return ThemePropertyType.Filename;
                case TMT.IMAGEFILE1:
                    return ThemePropertyType.Filename;
                case TMT.IMAGEFILE2:
                    return ThemePropertyType.Filename;
                case TMT.IMAGEFILE3:
                    return ThemePropertyType.Filename;
                case TMT.IMAGEFILE4:
                    return ThemePropertyType.Filename;
                case TMT.IMAGEFILE5:
                    return ThemePropertyType.Filename;
                case TMT.IMAGEFILE6:
                    return ThemePropertyType.Filename;
                case TMT.IMAGEFILE7:
                    return ThemePropertyType.Filename;

                case TMT.BODYFONT:
                    return ThemePropertyType.Font;
                case TMT.CAPTIONFONT:
                    return ThemePropertyType.Font;
                case TMT.GLYPHFONT:
                    return ThemePropertyType.Font;
                case TMT.HEADING1FONT:
                    return ThemePropertyType.Font;
                case TMT.HEADING2FONT:
                    return ThemePropertyType.Font;
                case TMT.ICONTITLEFONT:
                    return ThemePropertyType.Font;
                case TMT.MENUFONT:
                    return ThemePropertyType.Font;
                case TMT.MSGBOXFONT:
                    return ThemePropertyType.Font;
                case TMT.SMALLCAPTIONFONT:
                    return ThemePropertyType.Font;
                case TMT.STATUSFONT:
                    return ThemePropertyType.Font;

                case TMT.ALPHALEVEL:
                    return ThemePropertyType.Int;
                case TMT.ALPHATHRESHOLD:
                    return ThemePropertyType.Int;
                case TMT.ANIMATIONDELAY:
                    return ThemePropertyType.Int;
                case TMT.ANIMATIONDURATION:
                    return ThemePropertyType.Int;
                case TMT.BORDERSIZE:
                    return ThemePropertyType.Int;
                case TMT.CHARSET:
                    return ThemePropertyType.Int;
                case TMT.COLORIZATIONCOLOR:
                    return ThemePropertyType.Int;
                case TMT.COLORIZATIONOPACITY:
                    return ThemePropertyType.Int;
                case TMT.FRAMESPERSECOND:
                    return ThemePropertyType.Int;
                case TMT.FROMHUE1:
                    return ThemePropertyType.Int;
                case TMT.FROMHUE2:
                    return ThemePropertyType.Int;
                case TMT.FROMHUE3:
                    return ThemePropertyType.Int;
                case TMT.FROMHUE4:
                    return ThemePropertyType.Int;
                case TMT.FROMHUE5:
                    return ThemePropertyType.Int;
                case TMT.GLOWINTENSITY:
                    return ThemePropertyType.Int;
                case TMT.GLYPHINDEX:
                    return ThemePropertyType.Int;
                case TMT.GRADIENTRATIO1:
                    return ThemePropertyType.Int;
                case TMT.GRADIENTRATIO2:
                    return ThemePropertyType.Int;
                case TMT.GRADIENTRATIO3:
                    return ThemePropertyType.Int;
                case TMT.GRADIENTRATIO4:
                    return ThemePropertyType.Int;
                case TMT.GRADIENTRATIO5:
                    return ThemePropertyType.Int;
                case TMT.HEIGHT:
                    return ThemePropertyType.Int;
                case TMT.IMAGECOUNT:
                    return ThemePropertyType.Int;
                case TMT.MINCOLORDEPTH:
                    return ThemePropertyType.Int;
                case TMT.MINDPI1:
                    return ThemePropertyType.Int;
                case TMT.MINDPI2:
                    return ThemePropertyType.Int;
                case TMT.MINDPI3:
                    return ThemePropertyType.Int;
                case TMT.MINDPI4:
                    return ThemePropertyType.Int;
                case TMT.MINDPI5:
                    return ThemePropertyType.Int;
                case TMT.MINDPI6:
                    return ThemePropertyType.Int;
                case TMT.MINDPI7:
                    return ThemePropertyType.Int;
                case TMT.OPACITY:
                    return ThemePropertyType.Int;
                case TMT.PIXELSPERFRAME:
                    return ThemePropertyType.Int;
                case TMT.PROGRESSCHUNKSIZE:
                    return ThemePropertyType.Int;
                case TMT.PROGRESSSPACESIZE:
                    return ThemePropertyType.Int;
                case TMT.ROUNDCORNERHEIGHT:
                    return ThemePropertyType.Int;
                case TMT.ROUNDCORNERWIDTH:
                    return ThemePropertyType.Int;
                case TMT.SATURATION:
                    return ThemePropertyType.Int;
                case TMT.TEXTBORDERSIZE:
                    return ThemePropertyType.Int;
                case TMT.TEXTGLOWSIZE:
                    return ThemePropertyType.Int;
                case TMT.TOCOLOR1:
                    return ThemePropertyType.Int;
                case TMT.TOCOLOR2:
                    return ThemePropertyType.Int;
                case TMT.TOCOLOR3:
                    return ThemePropertyType.Int;
                case TMT.TOCOLOR4:
                    return ThemePropertyType.Int;
                case TMT.TOCOLOR5:
                    return ThemePropertyType.Int;
                case TMT.TOHUE1:
                    return ThemePropertyType.Int;
                case TMT.TOHUE2:
                    return ThemePropertyType.Int;
                case TMT.TOHUE3:
                    return ThemePropertyType.Int;
                case TMT.TOHUE4:
                    return ThemePropertyType.Int;
                case TMT.TOHUE5:
                    return ThemePropertyType.Int;
                case TMT.TRUESIZESTRETCHMARK:
                    return ThemePropertyType.Int;
                case TMT.WIDTH:
                    return ThemePropertyType.Int;

                case TMT.TRANSITIONDURATIONS:
                    return ThemePropertyType.IntList;

                case TMT.CAPTIONMARGINS:
                    return ThemePropertyType.Margins;
                case TMT.CONTENTMARGINS:
                    return ThemePropertyType.Margins;
                case TMT.SIZINGMARGINS:
                    return ThemePropertyType.Margins;

                case TMT.MINSIZE:
                    return ThemePropertyType.Position;
                case TMT.MINSIZE1:
                    return ThemePropertyType.Position;
                case TMT.MINSIZE2:
                    return ThemePropertyType.Position;
                case TMT.MINSIZE3:
                    return ThemePropertyType.Position;
                case TMT.MINSIZE4:
                    return ThemePropertyType.Position;
                case TMT.MINSIZE5:
                    return ThemePropertyType.Position;
                case TMT.MINSIZE6:
                    return ThemePropertyType.Position;
                case TMT.MINSIZE7:
                    return ThemePropertyType.Position;
                case TMT.NORMALSIZE:
                    return ThemePropertyType.Position;
                case TMT.OFFSET:
                    return ThemePropertyType.Position;
                case TMT.TEXTSHADOWOFFSET:
                    return ThemePropertyType.Position;
                case TMT.ANIMATIONBUTTONRECT:
                    return ThemePropertyType.Rect;
                case TMT.ATLASRECT:
                    return ThemePropertyType.Rect;
                case TMT.CUSTOMSPLITRECT:
                    return ThemePropertyType.Rect;
                case TMT.DEFAULTPANESIZE:
                    return ThemePropertyType.Rect;

                case TMT.CAPTIONBARHEIGHT:
                    return ThemePropertyType.Size;
                case TMT.CAPTIONBARWIDTH:
                    return ThemePropertyType.Size;
                case TMT.MENUBARHEIGHT:
                    return ThemePropertyType.Size;
                case TMT.MENUBARWIDTH:
                    return ThemePropertyType.Size;
                case TMT.PADDEDBORDERWIDTH:
                    return ThemePropertyType.Size;
                case TMT.SCROLLBARHEIGHT:
                    return ThemePropertyType.Size;
                case TMT.SCROLLBARWIDTH:
                    return ThemePropertyType.Size;
                case TMT.SIZINGBORDERWIDTH:
                    return ThemePropertyType.Size;
                case TMT.SMCAPTIONBARHEIGHT:
                    return ThemePropertyType.Size;
                case TMT.SMCAPTIONBARWIDTH:
                    return ThemePropertyType.Size;

                case TMT.ALIAS:
                    return ThemePropertyType.String;
                case TMT.ATLASINPUTIMAGE:
                    return ThemePropertyType.String;
                case TMT.AUTHOR:
                    return ThemePropertyType.String;
                case TMT.CLASSICVALUE:
                    return ThemePropertyType.String;
                case TMT.COLORSCHEMES:
                    return ThemePropertyType.String;
                case TMT.COMPANY:
                    return ThemePropertyType.String;
                case TMT.COPYRIGHT:
                    return ThemePropertyType.String;
                case TMT.CSSNAME:
                    return ThemePropertyType.String;
                case TMT.DESCRIPTION:
                    return ThemePropertyType.String;
                case TMT.DISPLAYNAME:
                    return ThemePropertyType.String;
                case TMT.LASTUPDATED:
                    return ThemePropertyType.String;
                case TMT.SIZES:
                    return ThemePropertyType.String;
                case TMT.TEXT:
                    return ThemePropertyType.String;
                case TMT.TOOLTIP:
                    return ThemePropertyType.String;
                case TMT.URL:
                    return ThemePropertyType.String;
                case TMT.VERSION:
                    return ThemePropertyType.String;
                case TMT.XMLNAME:
                    return ThemePropertyType.String;
                case TMT.NAME:
                    return ThemePropertyType.String;

                case TMT.ENUM:
                    return ThemePropertyType.Enum;
                case TMT.STRING:
                    return ThemePropertyType.String;
                case TMT.INT:
                    return ThemePropertyType.Int;
                case TMT.BOOL:
                    return ThemePropertyType.Bool;
                case TMT.COLOR:
                    return ThemePropertyType.Color;
                case TMT.MARGINS:
                    return ThemePropertyType.Margins;
                case TMT.FILENAME:
                    return ThemePropertyType.Filename;
                case TMT.SIZE:
                    return ThemePropertyType.Size;
                case TMT.POSITION:
                    return ThemePropertyType.Position;
                case TMT.RECT:
                    return ThemePropertyType.Rect;
                case TMT.FONT:
                    return ThemePropertyType.Font;
                case TMT.INTLIST:
                    return ThemePropertyType.IntList;
                case TMT.HBITMAP:
                    return ThemePropertyType.HBitmap;
                case TMT.DISKSTREAM:
                    return ThemePropertyType.DiskStream;
                case TMT.STREAM:
                    return ThemePropertyType.Stream;
                case TMT.BITMAPREF:
                    return ThemePropertyType.BitmapRef;
                case TMT.FLOAT:
                    return ThemePropertyType.Float;
                case TMT.FLOATLIST:
                    return ThemePropertyType.FloatList;

                case TMT.SIMPLIFIEDIMAGETYPE:
                    return ThemePropertyType.SimplifiedImage;
                case TMT.HCCOLOR:
                    return ThemePropertyType.Int;

                case TMT.RESERVEDLOW:
                case TMT.RESERVEDHIGH:
                default:
                    return ThemePropertyType.None;
            }
        }

        public static Tuple<int, string>[] Entries<T>()
        {
            return Entries(typeof(T));
        }

        public static Tuple<int, string>[] Entries(Type enumType)
        {
            var values = Enum.GetValues(enumType);
            var entries = new Tuple<int, string>[values.Length];
            for (int i = 0; i < values.Length; ++i) {
                var value = values.GetValue(i);
                string name = Enum.GetName(enumType, value);
                entries[i] = Tuple.Create((int)value, name);
            }

            return entries;
        }
    }
}
