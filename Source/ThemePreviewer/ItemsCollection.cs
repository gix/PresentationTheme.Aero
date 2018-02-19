namespace ThemePreviewer
{
    using System.Collections.ObjectModel;

    public sealed class ItemsCollection : ObservableCollection<Item>
    {
        public ItemsCollection()
        {
            Add(new Item(true, "Alice", "450 KB", "Image", "03.02.2010"));
            Add(new Item(true, "Bob", "5.584 KB", "Image", "13.02.2010"));
            Add(new Item(false, "Charlie", "23 KB", "Image", "11.05.2010"));
            Add(new Item(true, "Dave", "25.921 KB", "Image", "03.01.2010"));
            Add(new Item(false, "Eve", "1.271 KB", "Image", "16.03.2010"));
        }
    }

    public sealed class Item : ViewModel
    {
        private bool enabled;
        private string name;
        private string size;
        private string type;
        private string lastChanged;

        public Item(bool enabled, string name, string size, string type, string lastChanged)
        {
            Enabled = enabled;
            Name = name;
            Size = size;
            Type = type;
            LastChanged = lastChanged;
        }

        public bool Enabled
        {
            get => enabled;
            set => SetProperty(ref enabled, value);
        }

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public string Size
        {
            get => size;
            set => SetProperty(ref size, value);
        }

        public string Type
        {
            get => type;
            set => SetProperty(ref type, value);
        }

        public string LastChanged
        {
            get => lastChanged;
            set => SetProperty(ref lastChanged, value);
        }
    }
}
