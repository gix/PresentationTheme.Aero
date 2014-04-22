namespace ThemePreviewer
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public sealed class ItemsCollection : ObservableCollection<Item>
    {
        public ItemsCollection()
        {
            Add(new Item("Alice", "450 KB", "Image", "03.02.2010"));
            Add(new Item("Bob", "5.584 KB", "Image", "13.02.2010"));
            Add(new Item("Charlie", "23 KB", "Image", "11.05.2010"));
            Add(new Item("Dave", "25.921 KB", "Image", "03.01.2010"));
            Add(new Item("Eve", "1.271 KB", "Image", "16.03.2010"));
        }
    }

    public sealed class Item : INotifyPropertyChanged
    {
        private string name;
        private string size;
        private string type;
        private string lastChanged;

        public Item(string name, string size, string type, string lastChanged)
        {
            Name = name;
            Size = size;
            Type = type;
            LastChanged = lastChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get { return name; }
            set
            {
                if (name != value) {
                    name = value;
                    RaisePropertyChanged("Name");
                }
            }
        }

        public string Size
        {
            get { return size; }
            set
            {
                if (size != value) {
                    size = value;
                    RaisePropertyChanged("Size");
                }
            }
        }

        public string Type
        {
            get { return type; }
            set
            {
                if (type != value) {
                    type = value;
                    RaisePropertyChanged("Type");
                }
            }
        }

        public string LastChanged
        {
            get { return lastChanged; }
            set
            {
                if (lastChanged != value) {
                    lastChanged = value;
                    RaisePropertyChanged("LastChanged");
                }
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
