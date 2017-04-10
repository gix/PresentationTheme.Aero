namespace StyleInspector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using StyleCore;

    public class ThemeFileViewModel : IDisposable
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
        }

        public string FilePath => themeFile.FilePath;
        public string FileName => themeFile.FileName;
        public int Version => themeFile.Version;
        public IReadOnlyList<string> ClassNames => themeFile.ClassNames;
        public VariantMap VariantMap => themeFile.VariantMap;

        public IReadOnlyList<ThemeClassViewModel> Classes => classes;
        public IReadOnlyList<ThemePropertyViewModel> Properties => properties;
        public ThemeFile ThemeFile => themeFile;

        public void Dispose()
        {
            themeFile.Dispose();
        }
    }
}