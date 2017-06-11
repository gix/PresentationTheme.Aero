namespace ThemeBrowser
{
    using ThemeCore;
    using ThemeCore.Native;

    public class ThemeRawProperty
    {
        public ThemeRawProperty(
            int propId, string name, ThemePropertyType type, PropertyOrigin origin, object value)
        {
            PropId = propId;
            Name = name;
            Type = type;
            Origin = origin;
            Value = value;
        }

        public int PropId { get; }
        public string Name { get; }
        public ThemePropertyType Type { get; }
        public PropertyOrigin Origin { get; }
        public object Value { get; }
    }
}
