namespace StyleInspector
{
    using System.Collections.Generic;

    public class NamedItemContainer
    {
        public NamedItemContainer(string name, IReadOnlyList<object> items)
        {
            Name = name;
            Items = items;
        }

        public string Name { get; }
        public IReadOnlyList<object> Items { get; }
    }
}
