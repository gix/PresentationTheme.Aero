namespace ThemeBrowser
{
    using System.Collections.Generic;
    using System.Linq;
    using ThemeCore;
    using ThemeCore.Native;

    public class ThemePartViewModel : ThemePropertyContainer
    {
        private readonly List<ThemeStateViewModel> states;
        private readonly List<ThemePropertyViewModel> properties = new List<ThemePropertyViewModel>();

        public ThemePartViewModel(ThemePart part, ThemeClassViewModel parent)
        {
            Part = part;
            Parent = parent;

            properties.AddRange(part.Properties.Select(x => new OwnedThemePropertyViewModel(x)));

            states = part.States.Select(x => new ThemeStateViewModel(x, this)).ToList();
            states.Sort((x, y) => x.Id.CompareTo(y.Id));
        }

        public ThemePart Part { get; }
        public ThemeClassViewModel Parent { get; }
        public int Id => Part.Id;
        public string Name => Part.Name;
        public bool IsDefined => !Part.IsUndefined;

        public string DisplayName => Name != null ? Name + " [" + Id + "]" : "[Part " + Id + "]";
        public IReadOnlyList<ThemeStateViewModel> States => states;

        public override IReadOnlyList<ThemePropertyViewModel> Properties => properties;

        public ThemeStateViewModel FindState(int stateId)
        {
            return states.FirstOrDefault(x => x.Id == stateId);
        }

        public ThemeStateViewModel AddState(int stateId)
        {
            ThemeStateViewModel state = FindState(stateId);
            if (state == null) {
                state = new ThemeStateViewModel(Part.AddState(stateId), this);
                states.Add(state);
            }

            return state;
        }

        public void AddInheritedProperties()
        {
            foreach (var inherited in Parent.Properties)
                if (properties.All(x => x.PropertyId != inherited.PropertyId))
                    properties.Add(new InheritedThemePropertyViewModel(inherited));

            foreach (var state in States)
                state.AddInheritedProperties();
        }

        public void AddInheritedProperties(ThemePartViewModel basePart)
        {
            foreach (var inherited in basePart.Properties)
                if (properties.All(x => x.PropertyId != inherited.PropertyId))
                    properties.Add(new InheritedThemePropertyViewModel(inherited));

            foreach (var baseState in basePart.States) {
                var state = AddState(baseState.Id);
                state.AddInheritedProperties(baseState);
            }
        }

        public override void AddDefaultProperty(TMT propertyId, TMT primitiveType, object value)
        {
            var property = new ThemeProperty(Part, -1, -1, propertyId, primitiveType, PropertyOrigin.Default, value);
            properties.Add(new OwnedThemePropertyViewModel(property));
        }
    }
}
