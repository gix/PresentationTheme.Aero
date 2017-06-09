namespace StyleCore
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Native;

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGEPROPERTIES
    {
        public uint BorderColor;
        public uint BackgroundColor;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HCIMAGEPROPERTIES
    {
        public int BorderColor;
        public int BackgroundColor;
    }

    public class ThemeFileLoader
    {
        private readonly string filePath;
        private readonly SafeModuleHandle neutralModule;
        private readonly SafeModuleHandle muiModule;
        private readonly bool isHighContrast;

        public ThemeFileLoader(
            string filePath, SafeModuleHandle neutralModule,
            SafeModuleHandle muiModule, bool isHighContrast)
        {
            this.filePath = filePath;
            this.neutralModule = neutralModule;
            this.muiModule = muiModule;
            this.isHighContrast = isHighContrast;
        }

        private static string GetMUIPath(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string directory = Path.GetDirectoryName(filePath);
            directory = directory != null ? Path.Combine(directory, "en-US") : "en-US";
            return Path.Combine(directory, $"{fileName}.mui");
        }

        public static ThemeFile LoadTheme(string styleFilePath, bool isHighContrast)
        {
            var muiFilePath = GetMUIPath(styleFilePath);
            return LoadTheme(styleFilePath, isHighContrast, muiFilePath);
        }

        public static ThemeFile LoadTheme(string styleFilePath, bool isHighContrast, string muiFilePath)
        {
            SafeModuleHandle muiHandle = SafeModuleHandle.Zero;
            if (File.Exists(muiFilePath))
                muiHandle = SafeModuleHandle.LoadImageResource(muiFilePath);

            var styleHandle = SafeModuleHandle.LoadImageResource(styleFilePath);
            return new ThemeFileLoader(styleFilePath, styleHandle, muiHandle, isHighContrast).LoadTheme();
        }

        public ThemeFile LoadTheme()
        {
            var themeFile = new ThemeFile(filePath, neutralModule, muiModule);

            themeFile.Version = GetVersion();
            var classNames = ReadClassMap();
            var vmap = ReadVariantMap();
            var bcmap = ReadBaseClassMap();

            themeFile.VariantMap = vmap;
            themeFile.ClassNames = classNames;
            UxThemeExNativeMethods.UxOpenThemeFile(filePath, isHighContrast, out var themeFileHandle).ThrowIfFailed();
            themeFile.NativeThemeFile = themeFileHandle;

            ReadProperties(themeFile, "RMAP", "RMAP");
            ReadProperties(themeFile, "VARIANT", vmap.Name);

            foreach (var @class in themeFile.Classes)
                AddKnownPartsAndStates(@class);

            var classMap = themeFile.Classes.ToDictionary(x => x.Name);
            var classNames2 = themeFile.ClassNames.Skip(4).ToList();
            foreach (var entry in bcmap.Map) {
                var className = classNames2[entry.Key];
                var baseClassName = classNames2[entry.Value];

                if (classMap.TryGetValue(className, out ThemeClass @class) &&
                    classMap.TryGetValue(baseClassName, out ThemeClass baseClass))
                    @class.BaseClass = baseClass;
            }

            themeFile.Sort();

            return themeFile;
        }

        public short GetVersion()
        {
            using (var rh = neutralModule.FindResource("PACKTHEM_VERSION", 1)) {
                if (neutralModule.ResourceSize(rh) == 2)
                    return neutralModule.LoadResourceData<short>(rh);
            }

            return 0;
        }

        public VariantMap ReadVariantMap()
        {
            ResInfoHandle rh = ResourceUnsafeNativeMethods.FindResourceEx(
                neutralModule, "VMAP", "VMAP", 0);
            if (rh.IsInvalid)
                throw new InvalidOperationException("VMAP resource not found.");

            var stream = neutralModule.LoadResourceStream(rh);
            using (var reader = new BinaryReader(stream)) {
                var vmap = new VariantMap();
                vmap.Name = reader.ReadAlignedPascalZString(4);
                vmap.Size = reader.ReadAlignedPascalZString(4);
                vmap.Color = reader.ReadAlignedPascalZString(4);

                if (reader.BaseStream.Position != reader.BaseStream.Length)
                    throw new Exception("Trailing unread data in VMAP.");

                return vmap;
            }
        }

        public List<string> ReadClassMap()
        {
            ResInfoHandle rh = neutralModule.FindResourceEx("CMAP", "CMAP", 0);
            if (rh.IsInvalid)
                throw new InvalidOperationException("CMAP resource not found.");

            var classNames = new List<string>();

            var stream = neutralModule.LoadResourceStream(rh);
            using (var reader = new BinaryReader(stream)) {
                while (stream.Position < stream.Length) {
                    classNames.Add(reader.ReadZString());
                    reader.AlignTo(8);
                }
            }

            return classNames;
        }

        public BaseClassMap ReadBaseClassMap()
        {
            ResInfoHandle rh = neutralModule.FindResourceEx("BCMAP", "BCMAP", 0);
            if (rh.IsInvalid)
                return new BaseClassMap();

            var stream = neutralModule.LoadResourceStream(rh);
            using (var reader = new BinaryReader(stream)) {
                var bcmap = new BaseClassMap();

                int baseClassCount = reader.ReadInt32();
                for (int i = 0; i < baseClassCount; ++i)
                    bcmap.AddBaseClass(i, reader.ReadInt32());

                return bcmap;
            }
        }

        private void ReadProperties(ThemeFile themeFile, string type, string name)
        {
            ResInfoHandle rh = themeFile.Theme.FindResourceEx(type, name, 0);
            var data = themeFile.Theme.LoadResourceAccessor(rh);
            bool globals = false;

            ThemeClass cls = null;
            int recordCount = 0;
            int currClass = -1;
            for (long offset = 0; offset < data.Capacity;) {
                var recordOffset = offset;
                var recordId = recordCount++;
                var record = data.Read<VSRecord>(ref offset);
                if (cls == null || record.Class != currClass) {
                    string fullName = themeFile.ClassNames[record.Class];
                    string className = fullName;
                    ParseClassName(ref className, out string appName);

                    globals = className == "globals";

                    cls = themeFile.AddClass(fullName, appName, className);
                    currClass = record.Class;
                }

                object value;
                if (LoadRecordValue(themeFile, record, data, offset, out value)) {
                }

                var prop = cls.AddProperty(
                    recordId, recordOffset, record.Part, record.State,
                    record.Type, record.SymbolVal, value, globals);

                if (record.ResId == 0)
                    offset += record.ByteLength;

                DataExtensions.AlignTo(ref offset, 8);
            }
        }

        private void AddKnownPartsAndStates(ThemeClass cls)
        {
            foreach (var partInfo in ThemeInfo.GetParts(cls.ClassName)) {
                bool partDefined = cls.FindPart(partInfo.Item1) != null;
                var part = cls.AddPart(partInfo.Item1, partInfo.Item2);
                part.IsUndefined = !partDefined;

                foreach (var stateInfo in ThemeInfo.GetStates(part)) {
                    bool stateDefined = part.FindState(stateInfo.Item1) != null;
                    var state = part.AddState(stateInfo.Item1, stateInfo.Item2);
                    state.IsUndefined = !stateDefined;
                }
            }
        }

        private void ParseClassName(ref string className, out string appName)
        {
            int colon = className.IndexOf("::", StringComparison.Ordinal);
            if (colon != -1) {
                appName = className.Substring(0, colon);
                className = className.Substring(colon + 2);
            } else {
                appName = null;
            }
        }

        private TPID GetThemePrimitiveId(int symbolVal, int type)
        {
            TPID id = TPID.Invalid;
            for (int i = 0; i < propertyMap.Length; ++i) {
                var item = propertyMap[i];
                if (item.Type == type) {
                    id = (TPID)i;
                    if (item.PrimitiveId != 0 && (int)item.PrimitiveId == symbolVal)
                        break;
                }
            }

            return id;
        }

        private bool LoadRecordValue(
            ThemeFile theme, VSRecord record,
            UnmanagedMemoryAccessor data, long offset, out object value)
        {
            TPID id = GetThemePrimitiveId(record.SymbolVal, record.Type);
            if (id == TPID.Invalid) {
                value = null;
                return false;
            }

            if (record.ResId == 0) {
                var payload = new byte[record.ByteLength];
                data.ReadArray(offset, payload, 0, payload.Length);

                switch (id) {
                    case TPID.STRING:
                        value = data.ReadZString(offset);
                        return true;
                    case TPID.ENUM:
                        value = GetEnum(data.ReadInt32(offset), (TMT)record.SymbolVal);
                        return true;
                    case TPID.BOOL:
                        value = data.ReadInt32(offset) == 1;
                        return true;
                    case TPID.INT:
                        value = data.ReadInt32(offset);
                        return true;
                    case TPID.COLOR:
                        value = ColorUtils.ColorFromCOLORREF(data.ReadInt32(offset));
                        return true;
                    case TPID.MARGINS:
                        value = data.Read<MARGINS>(offset);
                        return true;
                    case TPID.SIZE:
                        value = data.ReadInt32(offset);
                        return true;
                    case TPID.POSITION:
                        value = data.Read<POINT>(offset);
                        return true;
                    case TPID.RECT:
                        value = data.Read<RECT>(offset);
                        return true;
                    case TPID.HIGHCONTRASTCOLORTYPE:
                        value = (HIGHCONTRASTCOLOR)data.ReadInt32(offset);
                        return true;
                    case TPID.SIMPLIFIEDIMAGETYPE:
                        if (record.SymbolVal == (int)TMT.HCSIMPLIFIEDIMAGE)
                            value = data.ReadArray<HCIMAGEPROPERTIES>(offset, record.ByteLength);
                        else
                            value = data.ReadArray<IMAGEPROPERTIES>(offset, record.ByteLength);
                        return true;
                    case TPID.INTLIST:
                        int length = data.ReadInt32(offset);
                        if (length <= 1) {
                            value = new IntList(new int[0]);
                            return true;
                        }

                        int dim = data.ReadInt32(offset + 4);
                        Debug.Assert(length - 1 == dim * dim);
                        var intList = new int[length - 1];
                        data.ReadArray(offset + 8, intList, 0, intList.Length);
                        value = new IntList(intList);
                        return true;
                    default:
                        Console.WriteLine($"Unprocessed resource property type {id}");
                        value = null;
                        return false;
                }
            } else {
                value = null;
                switch (id) {
                    case TPID.BITMAPIMAGE1:
                    case TPID.BITMAPIMAGE2:
                    case TPID.BITMAPIMAGE3:
                    case TPID.BITMAPIMAGE4:
                    case TPID.BITMAPIMAGE5:
                    case TPID.BITMAPIMAGE6:
                    case TPID.BITMAPIMAGE7:
                    case TPID.STOCKBITMAPIMAGE:
                    case TPID.GLYPHIMAGE:
                    case TPID.COMPOSEDIMAGETYPE:
                        return LoadImageFileRes(theme.Theme, record.ResId, out value);
                    // TPID_ATLASIMAGE
                    // TPID_ATLASINPUTIMAGE
                    // TPID_ENUM
                    case TPID.STRING:
                        return LoadStringRes(theme.MUI, record.ResId, out value);
                    // TPID_INT
                    case TPID.BOOL:
                        return LoadBoolRes(theme.MUI, record.ResId, out value);
                    // TPID_COLOR
                    // TPID_MARGINS
                    case TPID.FILENAME:
                        if (IsImageFile((TMT)record.SymbolVal))
                            return LoadImageFileRes(theme.Theme, record.ResId, out value);
                        return LoadStringRes(theme.MUI, record.ResId, out value);
                    // TPID_SIZE
                    // TPID_POSITION
                    case TPID.RECT:
                        return LoadRectRes(theme.MUI, record.ResId, out value);
                    case TPID.FONT:
                        return LoadFontRes(theme.MUI, record.ResId, out value);
                    case TPID.DISKSTREAM:
                        value = "DISKSTREAM(" + record.ResId + ", len=" + record.ByteLength + ")";
                        //var window = new Form();
                        //window.ShowDialog();
                        //var th = StyleNativeMethods.OpenThemeData(
                        //    window.Handle, "LISTVIEW");

                        //IntPtr stream;
                        //uint streamLen;
                        //HResult hr = StyleNativeMethods.GetThemeStream(
                        //    th, (int)header.Part, (int)header.State, (int)header.Property,
                        //    out stream, out streamLen, tf.hTheme);

                        //th.Dispose();

                        //GC.KeepAlive(window);
                        return true;
                    default:
                        Console.WriteLine($"Unprocessed MUI resource property type {id}");
                        return false;
                }
            }
        }

        private object GetEnum(int value, TMT symbolVal)
        {
            switch (symbolVal) {
                case TMT.BGTYPE: return (BGTYPE)value;
                case TMT.IMAGELAYOUT: return (IMAGELAYOUT)value;
                case TMT.BORDERTYPE: return (BORDERTYPE)value;
                case TMT.FILLTYPE: return (FILLTYPE)value;
                case TMT.SIZINGTYPE: return (SIZINGTYPE)value;
                case TMT.HALIGN: return (HALIGN)value;
                case TMT.CONTENTALIGNMENT: return (CONTENTALIGNMENT)value;
                case TMT.VALIGN: return (VALIGN)value;
                case TMT.OFFSETTYPE: return (OFFSETTYPE)value;
                case TMT.ICONEFFECT: return (ICONEFFECT)value;
                case TMT.TEXTSHADOWTYPE: return (TEXTSHADOWTYPE)value;
                case TMT.GLYPHTYPE: return (GLYPHTYPE)value;
                case TMT.IMAGESELECTTYPE: return (IMAGESELECTTYPE)value;
                case TMT.TRUESIZESCALINGTYPE: return (TRUESIZESCALINGTYPE)value;
                case TMT.GLYPHFONTSIZINGTYPE: return (GLYPHFONTSIZINGTYPE)value;
                case TMT.HCGLYPHBGCOLOR: return (HIGHCONTRASTCOLOR)value;
                default: return value;
            }
        }

        private bool LoadImageFileRes(SafeModuleHandle module, uint resId, out object value)
        {
            ResInfoHandle resInfo = module.FindResourceEx("IMAGE", (int)resId, 0);
            if (resInfo.IsInvalid) {
                value = null;
                return false;
            }

            value = new ThemeBitmap((int)resId, module, resInfo);
            return true;
        }

        private bool LoadFontRes(SafeModuleHandle module, uint resId, out object value)
        {
            if (module.IsInvalid) {
                value = "<<MUI missing>>";
                return false;
            }

            string str = ResourceUnsafeNativeMethods.LoadString(module, resId);

            if (TryParseFontSpec(str, out FontInfo info)) {
                value = info;
                return true;
            }

            value = null;
            return false;
        }

        private bool LoadStringRes(SafeModuleHandle module, uint resId, out object value)
        {
            if (module.IsInvalid) {
                value = "<<MUI missing>>";
                return false;
            }

            value = ResourceUnsafeNativeMethods.LoadString(module, resId);
            return true;
        }

        private bool LoadRectRes(SafeModuleHandle module, uint resId, out object value)
        {
            if (module.IsInvalid) {
                value = "<<MUI missing>>";
                return false;
            }

            string str = ResourceUnsafeNativeMethods.LoadString(module, resId);

            if (TryParseRectSpec(str, out RECT rect)) {
                value = rect;
                return true;
            }

            value = null;
            return false;
        }

        private bool LoadBoolRes(SafeModuleHandle module, uint resId, out object value)
        {
            if (module.IsInvalid) {
                value = "<<MUI missing>>";
                return false;
            }

            string str = ResourceUnsafeNativeMethods.LoadString(module, resId);
            if (str == "1" || string.Equals(str, "true", StringComparison.OrdinalIgnoreCase)) {
                value = true;
                return true;
            }
            if (str == "0" || string.Equals(str, "false", StringComparison.OrdinalIgnoreCase)) {
                value = false;
                return true;
            }

            value = null;
            return true;
        }

        private ThemeBitmap LoadReferencedImage(ThemeFile themeFile, int resRef)
        {
            ResInfoHandle resInfo = themeFile.Theme.FindResource("IMAGE", resRef);
            if (resInfo.IsInvalid)
                return null;

            return new ThemeBitmap(resRef, themeFile.Theme, resInfo);
        }

        class PROPERTYMAP
        {
            public PROPERTYMAP(TPID primitiveId, int symbolVal, int type, int typeByteLength)
            {
                PrimitiveId = primitiveId;
                SymbolVal = symbolVal;
                Type = type;
                TypeByteLength = typeByteLength;
            }

            public TPID PrimitiveId { get; }
            public int SymbolVal { get; }
            public int Type { get; }
            public int TypeByteLength { get; }
        }

        enum TPID
        {
            Invalid = -1,
            BITMAPIMAGE = 0,
            BITMAPIMAGE1 = 1,
            BITMAPIMAGE2 = 2,
            BITMAPIMAGE3 = 3,
            BITMAPIMAGE4 = 4,
            BITMAPIMAGE5 = 5,
            BITMAPIMAGE6 = 6,
            BITMAPIMAGE7 = 7,
            STOCKBITMAPIMAGE = 8,
            GLYPHIMAGE = 9,
            ATLASINPUTIMAGE = 0xA,
            ATLASIMAGE = 0xB,
            ENUM = 0xC,
            STRING = 0xD,
            INT = 0xE,
            BOOL = 0xF,
            COLOR = 0x10,
            MARGINS = 0x11,
            FILENAME = 0x12,
            SIZE = 0x13,
            POSITION = 0x14,
            RECT = 0x15,
            FONT = 0x16,
            INTLIST = 0x17,
            DISKSTREAM = 0x18,
            STREAM = 0x19,
            ANIMATION = 0x1A,
            TIMINGFUNCTION = 0x1B,
            SIMPLIFIEDIMAGETYPE = 0x1C,
            HIGHCONTRASTCOLORTYPE = 0x1D,
            BITMAPIMAGETYPE = 0x1E,
            COMPOSEDIMAGETYPE = 0x1F,
            FLOAT = 0x20,
            FLOATLIST = 0x21,
        }

        private readonly PROPERTYMAP[] propertyMap = {
            new PROPERTYMAP(TPID.BITMAPIMAGE, 0xBB9, 0xCE, 0x10),
            new PROPERTYMAP(TPID.BITMAPIMAGE1, 0xBBA, 0xCE, 0x10),
            new PROPERTYMAP(TPID.BITMAPIMAGE2, 0xBBB, 0xCE, 0x10),
            new PROPERTYMAP(TPID.BITMAPIMAGE3, 0xBBC, 0xCE, 0x10),
            new PROPERTYMAP(TPID.BITMAPIMAGE4, 0xBBD, 0xCE, 0x10),
            new PROPERTYMAP(TPID.BITMAPIMAGE5, 0xBBE, 0xCE, 0x10),
            new PROPERTYMAP(TPID.BITMAPIMAGE6, 0xBC1, 0xCE, 0x10),
            new PROPERTYMAP(TPID.BITMAPIMAGE7, 0xBC2, 0xCE, 0x10),
            new PROPERTYMAP(TPID.STOCKBITMAPIMAGE, 0xBBF, 0xCE, 0x10),
            new PROPERTYMAP(TPID.GLYPHIMAGE, 0xBC0, 0xCE, 0x10),
            new PROPERTYMAP(TPID.ATLASIMAGE, 0x1F40, 0xD5, -1),
            new PROPERTYMAP(TPID.ATLASINPUTIMAGE, 0x1F41, 0xC9, 0x10),
            new PROPERTYMAP(TPID.ENUM, 0, 0xC8, 4),
            new PROPERTYMAP(TPID.STRING, 0, 0xC9, -1),
            new PROPERTYMAP(TPID.INT, 0, 0xCA, 4),
            new PROPERTYMAP(TPID.BOOL, 0, 0xCB, 4),
            new PROPERTYMAP(TPID.COLOR, 0, 0xCC, 4),
            new PROPERTYMAP(TPID.MARGINS, 0, 0xCD, 0x10),
            new PROPERTYMAP(TPID.FILENAME, 0, 0xCE, -1),
            new PROPERTYMAP(TPID.SIZE, 0, 0xCF, 4),
            new PROPERTYMAP(TPID.POSITION, 0, 0xD0, 8),
            new PROPERTYMAP(TPID.RECT, 0, 0xD1, 0x10),
            new PROPERTYMAP(TPID.FONT, 0, 0xD2, 0x5C),
            new PROPERTYMAP(TPID.INTLIST, 0, 0xD3, -1),
            new PROPERTYMAP(TPID.DISKSTREAM, 0, 0xD5, -1),
            new PROPERTYMAP(TPID.STREAM, 0, 0xD6, -1),
            new PROPERTYMAP(TPID.ANIMATION, 0x4E20, 0xF1, -1),
            new PROPERTYMAP(TPID.TIMINGFUNCTION, 0x4E84, 0xF2, -1),
            new PROPERTYMAP(TPID.SIMPLIFIEDIMAGETYPE, 0, 0xF0, -1),
            new PROPERTYMAP(TPID.HIGHCONTRASTCOLORTYPE, 0, 0xF1, 4),
            new PROPERTYMAP(TPID.BITMAPIMAGETYPE, 0, 0xF2, 0x10),
            new PROPERTYMAP(TPID.COMPOSEDIMAGETYPE, 0, 0xF3, 0x10),
            new PROPERTYMAP(TPID.FLOAT, 0, 0xD8, 4),
            new PROPERTYMAP(TPID.FLOATLIST, 0, 0xD9, -1),
        };

        private static bool IsImageFile(TMT property)
        {
            return
                property == TMT.IMAGEFILE ||
                property == TMT.IMAGEFILE1 ||
                property == TMT.IMAGEFILE2 ||
                property == TMT.IMAGEFILE3 ||
                property == TMT.IMAGEFILE4 ||
                property == TMT.IMAGEFILE5 ||
                property == TMT.IMAGEFILE6 ||
                property == TMT.IMAGEFILE7 ||
                property == TMT.COMPOSEDIMAGEFILE1 ||
                property == TMT.COMPOSEDIMAGEFILE2 ||
                property == TMT.COMPOSEDIMAGEFILE3 ||
                property == TMT.COMPOSEDIMAGEFILE4 ||
                property == TMT.COMPOSEDIMAGEFILE5 ||
                property == TMT.COMPOSEDIMAGEFILE6 ||
                property == TMT.COMPOSEDIMAGEFILE7;
        }

        private bool TryParseRectSpec(string spec, out RECT rect)
        {
            rect = new RECT();

            var parts = spec.Split(new[] { ", " }, StringSplitOptions.None);
            if (parts.Length == 4 &&
                int.TryParse(parts[0], NumberStyles.None, null, out rect.left) &&
                int.TryParse(parts[1], NumberStyles.None, null, out rect.top) &&
                int.TryParse(parts[2], NumberStyles.None, null, out rect.right) &&
                int.TryParse(parts[3], NumberStyles.None, null, out rect.bottom))
                return true;

            return false;
        }

        private bool TryParseFontSpec(string spec, out FontInfo info)
        {
            var parts = spec.Split(new[] { ", " }, StringSplitOptions.None);
            if (parts.Length == 3 &&
                int.TryParse(parts[1], NumberStyles.None, null, out int pointSize)) {
                info = new FontInfo(parts[0], pointSize, parts[2]);
                return true;
            }

            info = null;
            return false;
        }
    }

    /// struct _VSRECORD
    /// {
    ///     int lSymbolVal;
    ///     int lType;
    ///     int iClass;
    ///     int iPart;
    ///     int iState;
    ///     uint uResID;
    ///     int lReserved;
    ///     int cbData;
    /// };
    [StructLayout(LayoutKind.Sequential)]
    internal struct VSRecord
    {
        public int SymbolVal;
        public int Type;
        public int Class;
        public int Part;
        public int State;
        public uint ResId;
        public int Reserved;
        public int ByteLength;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct _VSRECORD
    {
        public int lSymbolVal;
        public int lType;
        public int iClass;
        public int iPart;
        public int iState;
        public uint uResID;
        public int lReserved;
        public int cbData;
    }

    public class FontInfo
    {
        public FontInfo(string fontFamily)
        {
            FontFamily = fontFamily;
        }

        public FontInfo(string fontFamily, int pointSize, string options)
        {
            FontFamily = fontFamily;
            PointSize = pointSize;
            Options = options;
        }

        public string FontFamily { get; set; }
        public int PointSize { get; set; }
        public string Options { get; set; }

        public override string ToString()
        {
            var opt = String.IsNullOrEmpty(Options) ? ", " + Options : String.Empty;
            return $"{FontFamily} ({PointSize} pt{opt})";
        }
    }

    public class BaseClassMap
    {
        private const int InvalidClassId = -1;
        private readonly Dictionary<int, int> map = new Dictionary<int, int>();
        public IReadOnlyDictionary<int, int> Map => map;

        public void AddBaseClass(int classId, int baseClassId)
        {
            if (baseClassId != InvalidClassId)
                map.Add(classId, baseClassId);
        }
    }

    public class VariantMap
    {
        public string Name { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
    }

    public class ThemeFile : IDisposable
    {
        private readonly List<ThemeClass> classes = new List<ThemeClass>();
        private ThemeClass globals;
        private bool globalsSearched;

        public ThemeFile(string filePath, SafeModuleHandle style, SafeModuleHandle mui)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            Theme = style;
            MUI = mui;
        }

        public string FilePath { get; }
        public string FileName { get; }
        public SafeModuleHandle Theme { get; }
        public SafeModuleHandle MUI { get; }

        public IReadOnlyList<ThemeClass> Classes => classes;
        public int Version { get; set; } = -1;
        public List<string> ClassNames { get; set; }

        public VariantMap VariantMap { get; set; }

        public IReadOnlyList<ThemeProperty> Properties
        {
            get
            {
                if (!globalsSearched) {
                    foreach (var @class in classes) {
                        if (@class.Name.Equals("globals", StringComparison.OrdinalIgnoreCase)) {
                            globals = @class;
                            break;
                        }
                    }

                    globalsSearched = true;
                }

                return globals?.Properties ?? new ThemeProperty[0];
            }
        }

        public SafeThemeFileHandle NativeThemeFile { get; set; }

        public ThemeClass FindClass(string name)
        {
            foreach (var @class in Classes) {
                if (string.Equals(@class.Name, name, StringComparison.OrdinalIgnoreCase))
                    return @class;
            }

            return null;
        }

        public ThemeClass FindClass(string appName, string className)
        {
            foreach (var @class in Classes) {
                if (string.Equals(@class.AppName, appName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(@class.ClassName, className, StringComparison.OrdinalIgnoreCase))
                    return @class;
            }

            return null;
        }

        public ThemeClass AddClass(string name, string appName, string className)
        {
            ThemeClass @class = FindClass(appName, className);
            if (@class == null) {
                @class = new ThemeClass(this, name, appName, className);
                classes.Add(@class);
            }

            return @class;
        }

        public void Sort()
        {
            classes.Sort(CompareClass);
            foreach (var @class in Classes)
                @class.Sort();
        }

        private int CompareClass(ThemeClass l, ThemeClass r)
        {
            return string.Compare(l.Name, r.Name, StringComparison.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            Theme.Dispose();
            MUI.Dispose();
            NativeThemeFile?.Dispose();
        }

        public ThemeProperty FindProperty(TMT propertyId)
        {
            return Properties.FirstOrDefault(x => x.PrimitiveType == propertyId);
        }

        public T FindProperty<T>(TMT propertyId, T defaultValue)
            where T : struct
        {
            var property = Properties.FirstOrDefault(x => x.PropertyId == propertyId);
            if (property != null)
                return (T)property.Value;
            return defaultValue;
        }
    }

    public class ThemeClass : IPropertyOwner
    {
        private readonly List<ThemePart> parts = new List<ThemePart>();
        private readonly PropertyCollection properties = new PropertyCollection();
        private ThemePart classPart;

        public ThemeClass overrides;

        public ThemeClass(ThemeFile parent, string name, string appName, string className)
        {
            Parent = parent;
            Name = name;
            AppName = appName;
            ClassName = className;
        }

        public ThemeFile Parent { get; }
        public string Name { get; }
        public string AppName { get; }
        public string ClassName { get; }

        public ThemeClass BaseClass { get; set; }

        public IReadOnlyList<ThemePart> Parts => parts;
        public IReadOnlyList<ThemeProperty> Properties => properties;

        public ThemePart FindPart(int partId)
        {
            if (partId == 0)
                return classPart;

            foreach (var part in Parts) {
                if (part.Id == partId)
                    return part;
            }

            return overrides?.FindPart(partId);
        }

        public ThemePart AddPart(int partId, string partName = null)
        {
            ThemePart part = FindPart(partId);
            if (part != null)
                return part;

            part = new ThemePart(this, partId, partName ?? ThemeInfo.GetPartName(ClassName, partId));
            if (partId == 0)
                classPart = part;
            else
                parts.Add(part);
            return part;
        }

        public void Sort()
        {
            parts.Sort(ComparePart);
            foreach (var partState in Parts)
                partState.Sort();
        }

        private int ComparePart(ThemePart x, ThemePart y)
        {
            return x.Id.CompareTo(y.Id);
        }

        public ThemeProperty FindProperty(TMT propertyId)
        {
            return Properties.FirstOrDefault(x => x.PrimitiveType == propertyId)
                   ?? Parent.FindProperty(propertyId);
        }

        public T FindProperty<T>(TMT propertyId, T defaultValue)
            where T : struct
        {
            var property = Properties.FirstOrDefault(x => x.PropertyId == propertyId);
            if (property != null)
                return (T)property.Value;
            return Parent.FindProperty(propertyId, defaultValue);
        }

        public ThemeProperty AddProperty(
            int index, long offset, int partId, int stateId, int primitiveType,
            int propertyId, object value, bool isGlobal)
        {
            if (partId == 0) {
                var property = properties.FindProperty(primitiveType, propertyId);
                if (property != null)
                    return property;

                var origin = isGlobal ? PropertyOrigin.Global : PropertyOrigin.Class;
                property = new ThemeProperty(
                    this, index, offset, (TMT)propertyId, (TMT)primitiveType,
                    origin, value);

                properties.Add(property);

                return property;
            }

            var part = AddPart(partId);
            if (stateId == 0)
                return part.AddProperty(index, offset, primitiveType, propertyId, value);

            var state = part.AddState(stateId);
            return state.AddProperty(index, offset, primitiveType, propertyId, value);
        }
    }

    public class ThemePart : IPropertyOwner
    {
        private readonly List<ThemeState> states = new List<ThemeState>();
        private readonly PropertyCollection properties = new PropertyCollection();
        private ThemeState partState;

        public ThemePart(ThemeClass parent, int id, string name)
        {
            Parent = parent;
            Id = id;
            Name = name;
        }

        public ThemeClass Parent { get; }
        public int Id { get; }
        public string Name { get; }

        public IReadOnlyList<ThemeState> States => states;
        public IReadOnlyList<ThemeProperty> Properties => properties;
        public bool IsUndefined { get; set; }

        public ThemeState FindState(int stateId)
        {
            if (stateId == 0)
                return partState;

            return states.FirstOrDefault(x => x.Id == stateId);
        }

        public ThemeState AddState(int stateId, string name = null)
        {
            ThemeState state = FindState(stateId);
            if (state == null) {
                state = new ThemeState(this, stateId, name ?? ThemeInfo.GetStateName(Parent.ClassName, Id, stateId));
                if (stateId == 0)
                    partState = state;
                else
                    states.Add(state);
            }

            return state;
        }

        public ThemeProperty FindProperty(TMT propertyId)
        {
            return Properties.FirstOrDefault(x => x.PrimitiveType == propertyId)
                   ?? Parent.FindProperty(propertyId);
        }

        public T FindProperty<T>(TMT propertyId, T defaultValue)
            where T : struct
        {
            var property = Properties.FirstOrDefault(x => x.PropertyId == propertyId);
            if (property != null)
                return (T)property.Value;
            return Parent.FindProperty(propertyId, defaultValue);
        }

        public ThemeProperty AddProperty(
            int index, long offset, int primitiveType, int propertyId,
            object value)
        {
            var property = properties.FindProperty(primitiveType, propertyId);
            if (property != null)
                return property;

            property = new ThemeProperty(
                this, index, offset, (TMT)propertyId, (TMT)primitiveType,
                PropertyOrigin.Part, value);
            properties.Add(property);

            Debug.Assert(Id != 0);

            return property;
        }

        public void Sort()
        {
            states.Sort(CompareState);
        }

        private int CompareState(ThemeState x, ThemeState y)
        {
            return x.Id.CompareTo(y.Id);
        }
    }

    public class PropertyCollection : IReadOnlyList<ThemeProperty>
    {
        private readonly List<ThemeProperty> properties = new List<ThemeProperty>();

        public int Count => properties.Count;

        public ThemeProperty this[int index] => properties[index];

        public IEnumerator<ThemeProperty> GetEnumerator()
        {
            return properties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)properties).GetEnumerator();
        }

        public ThemeProperty FindProperty(int primitiveType, int propertyId)
        {
            foreach (var property in properties) {
                if (property.PropertyId == (TMT)propertyId) {
                    if (property.PrimitiveType == (TMT)primitiveType ||
                        primitiveType == 0)
                        return property;
                    return null;
                }
            }

            return null;
        }

        public void Add(ThemeProperty property)
        {
            properties.Add(property);
        }

        public void Sort(Comparison<ThemeProperty> comparison)
        {
            properties.Sort(comparison);
        }

        public void RemoveAt(int index)
        {
            properties.RemoveAt(index);
        }
    }

    public class ThemeState : IPropertyOwner
    {
        private readonly PropertyCollection properties = new PropertyCollection();

        public ThemeState(ThemePart parent, int id, string name)
        {
            Parent = parent;
            Id = id;
            Name = name;
        }

        public ThemePart Parent { get; }
        public int Id { get; }
        public string Name { get; }

        public IReadOnlyList<ThemeProperty> Properties => properties;
        public bool IsUndefined { get; set; }

        public ThemeProperty FindProperty(TMT propertyId)
        {
            return Properties.FirstOrDefault(x => x.PropertyId == propertyId)
                   ?? Parent.FindProperty(propertyId);
        }

        public T FindProperty<T>(TMT propertyId, T defaultValue)
            where T : struct
        {
            var property = Properties.FirstOrDefault(x => x.PropertyId == propertyId);
            if (property != null)
                return (T)property.Value;
            return Parent.FindProperty(propertyId, defaultValue);
        }

        public ThemeProperty AddProperty(
            int index, long offset, int primitiveType, int propertyId,
            object value)
        {
            var property = properties.FindProperty(primitiveType, propertyId);
            if (property != null)
                return property;

            property = new ThemeProperty(
                this, index, offset, (TMT)propertyId, (TMT)primitiveType,
                PropertyOrigin.State, value);
            properties.Add(property);

            Debug.Assert(Parent.Id != 0);
            Debug.Assert(Id != 0);

            return property;
        }

        private int CompareProperty(ThemeProperty x, ThemeProperty y)
        {
            return x.PropertyId.CompareTo(y.PropertyId);
        }
    }

    public interface IPropertyOwner
    {
        IReadOnlyList<ThemeProperty> Properties { get; }
    }

    [DebuggerDisplay("[{PropertyId}] {Value}")]
    public class ThemeProperty
    {
        public ThemeProperty(
            IPropertyOwner owner, int index, long offset, TMT propertyId, TMT primitiveType,
            PropertyOrigin origin, object value)
        {
            Owner = owner;
            Index = index;
            Offset = offset;
            PropertyId = propertyId;
            PrimitiveType = primitiveType;
            Origin = origin;
            Value = value;
        }

        public IPropertyOwner Owner { get; set; }
        public int Index { get; }
        public long Offset { get; }

        public TMT PropertyId { get; }
        public TMT PrimitiveType { get; }
        public PropertyOrigin Origin { get; }
        public object Value { get; }
    }

    public enum ThemePropertyType
    {
        Unknown = -1,
        None = 0,
        Enum = 200,
        String = 201,
        Int = 202,
        Bool = 203,
        Color = 204,
        Margins = 205,
        Filename = 206,
        Size = 207,
        Position = 208,
        Rect = 209,
        Font = 210,
        IntList = 211,
        HBitmap = 212,
        DiskStream = 213,
        Stream = 214,
        BitmapRef = 215,
        Float = 216,
        FloatList = 217,
        SimplifiedImage = 240,
    }

    public class ThemeBitmap
    {
        private readonly SafeModuleHandle module;
        private readonly ResInfoHandle resource;

        public ThemeBitmap(int imageId, SafeModuleHandle module, ResInfoHandle resource)
        {
            this.module = module;
            this.resource = resource;
            ImageId = imageId;
        }

        public int ImageId { get; }

        public Stream OpenStream()
        {
            return module.LoadResourceStream(resource);
        }
    }

    public static class ColorUtils
    {
        public static Color ColorFromCOLORREF(int value)
        {
            var b = (byte)((value >> 16) & 0xFF);
            var g = (byte)((value >> 8) & 0xFF);
            var r = (byte)((value >> 0) & 0xFF);
            return Color.FromArgb(0xFF, r, g, b);
        }

        public static Color ColorFromPremultipliedArgb(uint value)
        {
            return Color.FromPremultipliedArgb(value);
        }

        public static Color Unpremultiply(Color argb)
        {
            double a = argb.A / 255.0;
            double r = argb.R / 255.0;
            double g = argb.G / 255.0;
            double b = argb.B / 255.0;

            r /= a;
            g /= a;
            b /= a;

            var ba = (byte)Math.Round(a * 255);
            var br = (byte)Math.Round(r * 255);
            var bg = (byte)Math.Round(g * 255);
            var bb = (byte)Math.Round(b * 255);
            return Color.FromArgb(ba, br, bg, bb);
        }
    }

    public struct Color
    {
        private Color(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        public static Color FromArgb(uint value)
        {
            var a = (byte)((value >> 24) & 0xFF);
            var r = (byte)((value >> 16) & 0xFF);
            var g = (byte)((value >> 8) & 0xFF);
            var b = (byte)((value >> 0) & 0xFF);
            return FromArgb(a, r, g, b);
        }

        public static Color FromPremultipliedArgb(uint value)
        {
            return ColorUtils.Unpremultiply(FromArgb(value));
        }

        public static Color FromArgb(byte a, byte r, byte g, byte b)
        {
            return new Color(a, r, g, b);
        }

        public byte A { get; }
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public uint ToArgb()
        {
            return
                (uint)A << 24 |
                (uint)R << 16 |
                (uint)G << 8 |
                (uint)B << 0;
        }
    }
}
