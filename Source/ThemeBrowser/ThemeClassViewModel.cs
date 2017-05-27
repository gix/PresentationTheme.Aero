namespace ThemeBrowser
{
    using System.Collections.Generic;
    using System.Linq;
    using StyleCore;
    using StyleCore.Native;

    public class ThemeClassViewModel : ThemePropertyContainer
    {
        private readonly ThemeClass @class;
        private readonly List<object> children = new List<object>();
        private readonly List<ThemePartViewModel> parts;
        private readonly List<ThemePropertyViewModel> properties = new List<ThemePropertyViewModel>();

        private string name;
        private ThemeClassViewModel baseClass;

        public ThemeClassViewModel(ThemeClass @class, ThemeFileViewModel parent)
        {
            this.@class = @class;
            Parent = parent;

            properties.AddRange(@class.Properties.Select(x => new OwnedThemePropertyViewModel(x)));

            parts = @class.Parts.Select(x => new ThemePartViewModel(x, this)).ToList();
            parts.Sort((x, y) => x.Id.CompareTo(y.Id));
            children.Add(new NamedItemContainer("Parts", parts));
        }

        public ThemeFileViewModel Parent { get; }
        public ThemeClass Class => @class;
        public string Name => name ?? (name = AppName != null ? string.Join("::", AppName, ClassName) : ClassName);
        public string AppName => @class.AppName;
        public string ClassName => @class.ClassName;

        public ThemeClassViewModel BaseClass
        {
            get => baseClass;
            set
            {
                if (baseClass != null)
                    children.RemoveAt(0);

                baseClass = value;
                if (baseClass != null)
                    children.Insert(0, baseClass);
            }
        }

        private bool inheritedPropertiesAdded;

        public void AddInheritedProperties()
        {
            if (inheritedPropertiesAdded)
                return;

            foreach (var part in Parts)
                part.AddInheritedProperties();

            if (BaseClass != null) {
                BaseClass.AddInheritedProperties();
                foreach (var inherited in BaseClass.Properties) {
                    if (properties.All(x => x.PropertyId != inherited.PropertyId))
                        properties.Add(new InheritedThemePropertyViewModel(inherited));
                }

                foreach (var basePart in BaseClass.Parts) {
                    var part = AddPart(basePart.Id, basePart.Name);

                    part.AddInheritedProperties(basePart);
                }
            }

            inheritedPropertiesAdded = true;
        }

        public void RemoveInheritedProperties()
        {
            if (!inheritedPropertiesAdded)
                return;

            for (int i = properties.Count - 1; i >= 0; --i) {
                if (properties[i] is InheritedThemePropertyViewModel)
                    properties.RemoveAt(i);
            }

            inheritedPropertiesAdded = false;
        }

        public IReadOnlyList<object> Children => children;

        public IReadOnlyList<ThemePartViewModel> Parts => parts;

        public override IReadOnlyList<ThemePropertyViewModel> Properties => properties;

        public ThemePartViewModel FindPart(int partId)
        {
            return Parts.FirstOrDefault(part => part.Id == partId);
        }

        public ThemePartViewModel AddPart(int partId, string partName = null)
        {
            ThemePartViewModel part = FindPart(partId);
            if (part == null) {
                part = new ThemePartViewModel(Class.AddPart(partId), this);
                parts.Add(part);
            }

            return part;
        }

        public override void AddDefaultProperty(TMT propertyId, TMT primitiveType, object value)
        {
            var property = new ThemeProperty(Class, -1, -1, propertyId, primitiveType, PropertyOrigin.Default, value);
            properties.Add(new OwnedThemePropertyViewModel(property));
        }
    }
}
