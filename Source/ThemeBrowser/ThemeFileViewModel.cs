namespace ThemeBrowser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ThemeCore;
    using ThemeCore.Native;

    public abstract class ThemeFileBase : ViewModel, IDisposable
    {
        public abstract void Dispose();
    }

    public class ThemeFileViewModel : ThemeFileBase
    {
        private readonly ThemeFile themeFile;
        private readonly List<ThemeClassViewModel> classes;
        private readonly List<ThemePropertyViewModel> properties = new List<ThemePropertyViewModel>();
        private readonly ThemeClassViewModel globals;

        public ThemeFileViewModel(ThemeFile themeFile)
        {
            this.themeFile = themeFile;

            properties.AddRange(themeFile.Properties.Select(x => new OwnedThemePropertyViewModel(x)));

            classes = themeFile.Classes.Select(x => new ThemeClassViewModel(x, this)).ToList();
            classes.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));

            var classMap = classes.ToDictionary(x => x.Class);
            foreach (var classViewModel in classes) {
                if (classViewModel.Class.BaseClass != null)
                    classViewModel.BaseClass = classMap[classViewModel.Class.BaseClass];
            }

            foreach (var @class in classes) {
                if (@class.Name.Equals("globals", StringComparison.OrdinalIgnoreCase)) {
                    globals = @class;
                    break;
                }
            }

            foreach (var @class in Classes)
                @class.AddInheritedProperties();

            AddDefaultProperties();
        }

        public string FilePath => themeFile.FilePath;
        public string FileName => themeFile.FileName;
        public int Version => themeFile.Version;
        public IReadOnlyList<string> ClassNames => themeFile.ClassNames;
        public VariantMap VariantMap => themeFile.VariantMap;

        public IReadOnlyList<ThemeClassViewModel> Classes => classes;
        public IReadOnlyList<ThemePropertyViewModel> Properties => properties;
        public ThemeFile ThemeFile => themeFile;

        public override void Dispose()
        {
            themeFile.Dispose();
        }

        private void AddDefaultProperties()
        {
            foreach (var @class in Classes) {
                AddDefaultProperties(@class);
                foreach (var part in @class.Parts) {
                    AddDefaultProperties(part);
                    foreach (var state in part.States)
                        AddDefaultProperties(state);
                }
            }
        }

        private void AddDefaultProperties(ThemePropertyContainer container)
        {
            BGTYPE bgType = container.EnsurePropertyValue(TMT.BGTYPE, TMT.ENUM, BGTYPE.BT_BORDERFILL);
            if (bgType == BGTYPE.BT_BORDERFILL)
                AddDefaultBorderFillProperties(container);
            else if (bgType == BGTYPE.BT_IMAGEFILE)
                AddDefaultImageFileProperties(container);

            AddDefaultTextDrawPackProperties(container);
        }

        private void AddDefaultImageFileProperties(ThemePropertyContainer container)
        {
            container.EnsureIntValue(TMT.IMAGECOUNT, 1);
            container.EnsureEnumValue(TMT.IMAGELAYOUT, IMAGELAYOUT.IL_HORIZONTAL);

            container.EnsureEnumValue(TMT.TRUESIZESCALINGTYPE, TRUESIZESCALINGTYPE.TSST_NONE);
            container.EnsureEnumValue(TMT.SIZINGTYPE, SIZINGTYPE.ST_STRETCH);
            container.EnsureEnumValue(TMT.TRUESIZESCALINGTYPE, TRUESIZESCALINGTYPE.TSST_NONE);
            container.EnsureBoolValue(TMT.BORDERONLY, false);
            container.EnsureIntValue(TMT.TRUESIZESTRETCHMARK, 0);
            container.EnsureBoolValue(TMT.UNIFORMSIZING, false);
            container.EnsureBoolValue(TMT.INTEGRALSIZING, false);
            container.EnsureBoolValue(TMT.MIRRORIMAGE, true);
            container.EnsureEnumValue(TMT.HALIGN, HALIGN.HA_CENTER);
            container.EnsureEnumValue(TMT.VALIGN, VALIGN.VA_CENTER);

            if (container.HasProperty(TMT.BGFILL))
                container.EnsureColorValue(TMT.FILLCOLOR, Color.FromArgb(0xFFFFFF));

            var sizingMargins = container.EnsureMarginsValue(TMT.SIZINGMARGINS, new MARGINS());
            container.EnsureMarginsValue(TMT.CONTENTMARGINS, sizingMargins);
            container.EnsureBoolValue(TMT.SOURCEGROW, false);
            container.EnsureBoolValue(TMT.SOURCESHRINK, false);

            var glyphType = container.EnsureEnumValue(TMT.GLYPHTYPE, GLYPHTYPE.GT_NONE);

            if (glyphType == GLYPHTYPE.GT_FONTGLYPH) {
                container.EnsureColorValue(TMT.GLYPHTEXTCOLOR, Color.FromArgb(0));
                container.EnsureIntValue(TMT.GLYPHINDEX, 1);
            }

            if (glyphType != 0)
                container.EnsureBoolValue(TMT.GLYPHONLY, false);

            container.EnsureEnumValue(TMT.IMAGESELECTTYPE, IMAGESELECTTYPE.IST_NONE);
        }

        private void AddDefaultBorderFillProperties(ThemePropertyContainer container)
        {
            var borderType = container.EnsureEnumValue(TMT.BORDERTYPE, BORDERTYPE.BT_RECT);
            container.EnsureColorValue(TMT.BORDERCOLOR, Color.FromArgb(0));
            var borderSize = container.EnsureIntValue(TMT.BORDERSIZE, 1);

            if (borderType == BORDERTYPE.BT_ROUNDRECT) {
                container.EnsureIntValue(TMT.ROUNDCORNERWIDTH, 80);
                container.EnsureIntValue(TMT.ROUNDCORNERHEIGHT, 80);
            }

            var fillType = container.EnsureEnumValue(TMT.FILLTYPE, FILLTYPE.FT_SOLID);

            switch (fillType) {
                case FILLTYPE.FT_SOLID:
                    container.EnsureColorValue(TMT.FILLCOLOR, Color.FromArgb(0xFFFFFF));
                    break;
                case FILLTYPE.FT_HORZGRADIENT:
                case FILLTYPE.FT_VERTGRADIENT:
                case FILLTYPE.FT_RADIALGRADIENT:
                    for (var prop = TMT.GRADIENTRATIO1; prop < TMT.GRADIENTRATIO5; ++prop) {
                        if (container.HasProperty(prop + 1404))
                            container.EnsureIntValue(prop, 0);
                    }
                    break;
            }

            container.EnsureMarginsValue(TMT.CONTENTMARGINS, new MARGINS {
                cxLeftWidth = borderSize,
                cxRightWidth = borderSize,
                cyTopHeight = borderSize,
                cyBottomHeight = borderSize
            });
        }

        private void AddDefaultTextDrawPackProperties(ThemePropertyContainer container)
        {
            container.EnsureColorValue(TMT.TEXTCOLOR, Color.FromArgb(0));

            if (container.HasProperty(TMT.TEXTSHADOWOFFSET)) {
                container.EnsureColorValue(TMT.TEXTSHADOWCOLOR, Color.FromArgb(0));
                container.EnsureEnumValue(TMT.TEXTSHADOWTYPE, TEXTSHADOWTYPE.TST_NONE);
            }

            if (!container.HasProperty(TMT.TEXTBORDERSIZE))
                container.EnsureIntValue(TMT.TEXTBORDERSIZE, 0);
            else {
                container.EnsureColorValue(TMT.TEXTBORDERCOLOR, Color.FromArgb(0));
                container.EnsureBoolValue(TMT.TEXTAPPLYOVERLAY, false);
            }

            container.EnsureIntValue(TMT.TEXTGLOWSIZE, 0);
            container.EnsureIntValue(TMT.GLOWINTENSITY, 0);
            container.EnsureColorValue(TMT.GLOWCOLOR, Color.FromArgb(0xFFFFFF));

            container.EnsureBoolValue(TMT.TEXTITALIC, false);

            var edgeLight = container.EnsureColorValue(TMT.EDGELIGHTCOLOR, Color.FromArgb(0xC0C0C0));
            container.EnsureColorValue(TMT.EDGEHIGHLIGHTCOLOR, Color.FromArgb(0xFFFFFF));
            container.EnsureColorValue(TMT.EDGESHADOWCOLOR, Color.FromArgb(0x808080));
            container.EnsureColorValue(TMT.EDGEDKSHADOWCOLOR, Color.FromArgb(0));
            container.EnsureColorValue(TMT.EDGEFILLCOLOR, edgeLight);
            container.EnsureBoolValue(TMT.COMPOSITED, false);
        }
    }
}
