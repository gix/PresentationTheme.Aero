namespace ThemeBrowser
{
    using System.Collections.Generic;
    using System.Linq;
    using StyleCore;
    using StyleCore.Native;

    public class ThemeStateViewModel : ThemePropertyContainer
    {
        private readonly ThemeState state;
        private readonly List<ThemePropertyViewModel> properties = new List<ThemePropertyViewModel>();
        private readonly CombinedList<ThemePropertyViewModel> allProperties;

        public ThemeStateViewModel(ThemeState state, ThemePartViewModel parent)
        {
            this.state = state;
            Parent = parent;

            properties.AddRange(state.Properties.Select(x => new OwnedThemePropertyViewModel(x)));
            allProperties = new CombinedList<ThemePropertyViewModel>(properties, Parent.AllProperties);
        }

        public ThemeState State => state;
        public ThemePartViewModel Parent { get; }
        public int Id => state.Id;
        public string Name => state.Name;
        public bool IsDefined => !state.IsUndefined;

        public string DisplayName => Name != null ? Name + " [" + Id + "]" : "[State " + Id + "]";

        public override IReadOnlyList<ThemePropertyViewModel> Properties => properties;
        public override IReadOnlyList<ThemePropertyViewModel> AllProperties => allProperties;

        public void AddInheritedProperties()
        {
            foreach (var inherited in Parent.Properties)
                if (properties.All(x => x.PropertyId != inherited.PropertyId))
                    properties.Add(new InheritedThemePropertyViewModel(inherited));
        }

        public void AddInheritedProperties(ThemeStateViewModel baseState)
        {
            foreach (var inherited in baseState.Properties)
                if (properties.All(x => x.PropertyId != inherited.PropertyId))
                    properties.Add(new InheritedThemePropertyViewModel(inherited));
        }

        public override void AddDefaultProperty(TMT propertyId, TMT primitiveType, object value)
        {
            var property = new ThemeProperty(State, -1, -1, propertyId, primitiveType, PropertyOrigin.Default, value);
            properties.Add(new OwnedThemePropertyViewModel(property));
        }
    }
}
