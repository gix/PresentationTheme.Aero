namespace ThemeTestApp.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for WpfTreeViewSample.xaml
    /// </summary>
    public partial class WpfTreeViewSample : IOptionControl
    {
        private readonly OptionList options;

        public WpfTreeViewSample()
        {
            InitializeComponent();
            DataContext = this;
            Items = new ObservableCollection<TreeItem>();

            var solutionNode = new TreeItem("Solution 'ThemeTestApp' (1 project)");
            var projectNode = new TreeItem("ThemeTestApp", solutionNode);
            var refNode = new TreeItem("References", projectNode);
            var resourcesNode = new TreeItem("Resources", projectNode);

            solutionNode.Children.Add(projectNode);
            projectNode.Children.Add(new TreeItem("Properties", projectNode));
            projectNode.Children.Add(refNode);
            refNode.Children.Add(new TreeItem("System.Core", refNode));
            projectNode.Children.Add(resourcesNode);
            resourcesNode.Children.Add(new TreeItem("Styles.xaml", resourcesNode));
            projectNode.Children.Add(new TreeItem("Program.cs", projectNode));
            solutionNode.Children.Add(new TreeItem("ThemeTestApp.Tests", solutionNode));

            Items.Add(solutionNode);

            options = new OptionList();
            options.AddOption(treeView1, "Enabled", l => l.IsEnabled);
        }

        public IReadOnlyList<Option> Options
        {
            get { return options; }
        }

        public ObservableCollection<TreeItem> Items { get; private set; }
    }

    public sealed class TreeItem : INotifyPropertyChanged
    {
        private bool isSelected;
        private bool isExpanded;
        private TreeItem parent;
        private string value;
        private ObservableCollection<TreeItem> children = new ObservableCollection<TreeItem>();

        public TreeItem(string value, TreeItem parent = null)
        {
            this.value = value;
            this.parent = parent;
        }

        public ObservableCollection<TreeItem> Children
        {
            get { return children; }
            private set
            {
                if (children != value) {
                    children = value;
                    RaisePropertyChanged("Children");
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
                    RaisePropertyChanged("Value");
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
                    RaisePropertyChanged("IsSelected");
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
                    this.RaisePropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (isExpanded && parent != null)
                    parent.IsExpanded = true;
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
