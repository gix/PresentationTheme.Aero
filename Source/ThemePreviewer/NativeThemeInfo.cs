namespace ThemePreviewer
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public class NativeThemeInfo : IComparable<NativeThemeInfo>
    {
        public NativeThemeInfo(
            string name, FileVersionInfo version, FileInfo path,
            UxThemeLoadParams loadParams = null)
        {
            Name = name + " (" + version.GetWindowsName() + ")";
            Version = version;
            Path = path;
            LoadParams = loadParams ?? new UxThemeLoadParams();
        }

        public static NativeThemeInfo FromPath(string themePath, UxThemeLoadParams loadParams = null)
        {
            string name = System.IO.Path.GetFileName(themePath);
            var version = FileVersionInfo.GetVersionInfo(themePath);
            var path = new FileInfo(themePath);
            if (!path.Exists)
                throw new ArgumentException();
            return new NativeThemeInfo(name, version, path, loadParams);
        }

        public string Name { get; }
        public FileVersionInfo Version { get; }
        public FileInfo Path { get; }
        public UxThemeLoadParams LoadParams { get; }

        public string GetFullName()
        {
            string revision =
                $"{Version.FileMajorPart}.{Version.FileMinorPart}.{Version.FileBuildPart}.{Version.FilePrivatePart}";
            return $"{Name} [{revision}]";
        }

        public int CompareTo(NativeThemeInfo other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var cmp = String.Compare(Name, other.Name, StringComparison.Ordinal);
            if (cmp != 0)
                return cmp;
            cmp = (LoadParams?.IsHighContrast ?? false).CompareTo(other.LoadParams?.IsHighContrast ?? false);
            if (cmp != 0)
                return cmp;
            cmp = (LoadParams?.CustomColors != null).CompareTo(other.LoadParams?.CustomColors != null);
            if (cmp != 0)
                return cmp;
            return CompareFileVersion(Version, other.Version);
        }

        private static int CompareFileVersion(FileVersionInfo lhs, FileVersionInfo rhs)
        {
            if (ReferenceEquals(lhs, rhs)) return 0;
            if (ReferenceEquals(null, rhs)) return 1;
            var cmp = lhs.FileMajorPart.CompareTo(rhs.FileMajorPart);
            if (cmp != 0)
                return cmp;
            cmp = lhs.FileMinorPart.CompareTo(rhs.FileMinorPart);
            if (cmp != 0)
                return cmp;
            cmp = lhs.FileBuildPart.CompareTo(rhs.FileBuildPart);
            if (cmp != 0)
                return cmp;
            return lhs.FilePrivatePart.CompareTo(rhs.FilePrivatePart);
        }

        public bool Matches(string themePath, UxThemeLoadParams loadParams)
        {
            return
                string.Equals(Path.FullName, themePath, StringComparison.OrdinalIgnoreCase) &&
                LoadParams == loadParams;
        }
    }
}
