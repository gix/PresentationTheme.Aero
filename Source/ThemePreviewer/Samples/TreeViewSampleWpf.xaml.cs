namespace ThemePreviewer.Samples
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Controls;
    using PresentationTheme.Aero;

    public partial class TreeViewSampleWpf : IOptionControl
    {
        private readonly OptionList options;

        public TreeViewSampleWpf()
        {
            InitializeComponent();
            DataContext = this;

            var root = ItemGenerator.GetTree();
            AddItem(treeView1.Items, root);
            AddItem(treeView2.Items, root);

            options = new OptionList();
            options.AddOption("Enabled", treeView1, treeView2, l => l.IsEnabled);
            options.Add(new GenericOption(
                "FullRowSelect",
                () => TreeViewService.GetFullRowSelect(treeView1),
                v => {
                    TreeViewService.SetFullRowSelect(treeView1, v);
                    TreeViewService.SetFullRowSelect(treeView2, v);
                }));
        }

        private void AddItem(ItemCollection items, ItemGenerator.TreeNode root)
        {
            var item = new TreeViewItem {
                Header = root.Name,
                IsExpanded = true
            };

            foreach (var child in root.Children)
                AddItem(item.Items, child);

            items.Add(item);
        }

        public IReadOnlyList<Option> Options => options;
    }

    public static class TreeViewExtensions
    {
        public static IEnumerable<TreeViewItem> Containers(this TreeView treeView)
        {
            foreach (var item in treeView.Items) {
                using (treeView.ItemContainerGenerator.GenerateBatches()) {
                }

                var container = (TreeViewItem)treeView.ItemContainerGenerator.ContainerFromItem(item);
                if (container != null)
                    yield return container;
            }
        }

        public static IEnumerable<TreeViewItem> Containers(this TreeViewItem treeViewItem)
        {
            foreach (var item in treeViewItem.Items) {
                using (treeViewItem.ItemContainerGenerator.GenerateBatches()) {
                }

                var container = (TreeViewItem)treeViewItem.ItemContainerGenerator.ContainerFromItem(item);
                if (container != null)
                    yield return container;
            }
        }

        public static void ExpandAll(this TreeView treeView, bool isExpanded = true)
        {
            var stack = new Stack<TreeViewItem>(treeView.Containers());
            while (stack.Count > 0) {
                var item = stack.Pop();
                item.IsExpanded = isExpanded;

                foreach (var child in item.Containers())
                    stack.Push(child);
            }
        }

        public static void ExpandAll(this TreeViewItem treeViewItem, bool isExpanded = true)
        {
            var stack = new Stack<TreeViewItem>(treeViewItem.Containers());
            while (stack.Count > 0) {
                var item = stack.Pop();
                item.IsExpanded = isExpanded;

                foreach (var child in item.Containers())
                    stack.Push(child);
            }
        }

        public static void CollapseAll(this TreeView treeView)
        {
            treeView.ExpandAll(false);
        }

        public static void CollapseAll(this TreeViewItem treeViewItem)
        {
            treeViewItem.ExpandAll(false);
        }
    }

    public sealed class TreeItem : INotifyPropertyChanged
    {
        private TreeItem parent;
        private bool isSelected;
        private bool isExpanded = true;
        private string value;
        private ObservableCollection<TreeItem> children = new ObservableCollection<TreeItem>();

        public TreeItem(string value, TreeItem parent = null)
        {
            this.value = value;
            this.parent = parent;
            parent?.Children.Add(this);
        }

        public TreeItem Parent
        {
            get { return parent; }
            set
            {
                if (ReferenceEquals(parent, value))
                    return;

                parent?.Children.Remove(this);
                parent = value;
                parent?.Children.Add(this);
            }
        }

        public ObservableCollection<TreeItem> Children
        {
            get { return children; }
            private set
            {
                if (children != value) {
                    children = value;
                    RaisePropertyChanged(nameof(Children));
                }
            }
        }

        public string Name { get; set; }

        public string Value
        {
            get { return value; }
            set
            {
                if (this.value != value) {
                    this.value = value;
                    RaisePropertyChanged(nameof(Value));
                }
            }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (value != isSelected) {
                    isSelected = value;
                    RaisePropertyChanged(nameof(IsSelected));
                }
            }
        }

        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (value != isExpanded) {
                    isExpanded = value;
                    RaisePropertyChanged(nameof(IsExpanded));
                }

                // Expand all the way up to the root.
                if (isExpanded && parent != null)
                    parent.IsExpanded = true;
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
