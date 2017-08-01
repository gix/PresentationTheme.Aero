namespace ThemeBrowser
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Data;

    public partial class PropertyGrid
    {
        private readonly CollectionViewSource propertiesViewSource;

        public PropertyGrid()
        {
            InitializeComponent();
            propertiesViewSource = (CollectionViewSource)FindResource("PropertiesViewSource");
            IdColumn.SortDirection = ListSortDirection.Ascending;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(
            object sender, DependencyPropertyChangedEventArgs e)
        {
            var container = e.NewValue as ThemePropertyContainer;

            propertiesViewSource.Source = container?.Properties;
            if (container == null)
                return;

            var view = propertiesViewSource.View;
            Debug.Assert(view == DataGrid.ItemsSource as ICollectionView);
            Debug.Assert(view != null);

            if (view.SortDescriptions.Count == 0) {
                using (view.DeferRefresh()) {
                    view.SortDescriptions.Add(
                        new SortDescription(nameof(ThemePropertyViewModel.DisplayName), ListSortDirection.Ascending));
                    IdColumn.SortDirection = ListSortDirection.Ascending;
                }
            }
        }
    }
}
