namespace ThemePreviewer
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Interactivity;
    using Microsoft.Win32;

    public class CacheTabsBehavior : Behavior<TabControl>
    {
        private Panel itemsPanel;
        private ContentPresenter currContentHost;

        public CacheTabsBehavior()
        {
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
        }

        private void OnUserPreferenceChanged(
            object sender, UserPreferenceChangedEventArgs args)
        {
            if (args.Category == UserPreferenceCategory.VisualStyle ||
                args.Category == UserPreferenceCategory.Color)
                InvalidateCache();
        }

        private void InvalidateCache()
        {
            if (itemsPanel == null)
                return;

            itemsPanel.Children.Clear();
            UpdateSelectedContent();
        }

        private ContentPresenter ContentHost
        {
            get
            {
                var contentHost = AssociatedObject.Template?.FindName(
                    "PART_SelectedContentHost", AssociatedObject) as ContentPresenter;

                if (currContentHost != null &&
                    !ReferenceEquals(contentHost, currContentHost))
                    currContentHost.Content = null;

                currContentHost = contentHost;
                return currContentHost;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
            AssociatedObject.SelectionChanged += OnSelectionChanged;
            ((INotifyCollectionChanged)AssociatedObject.Items).CollectionChanged += OnItemsChanged;

            itemsPanel = new Grid();
            UpdateSelectedContent();
        }

        protected override void OnDetaching()
        {
            ((INotifyCollectionChanged)AssociatedObject.Items).CollectionChanged -= OnItemsChanged;
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
            AssociatedObject.ItemContainerGenerator.StatusChanged -= OnGeneratorStatusChanged;

            if (currContentHost != null)
                currContentHost.Content = AssociatedObject.SelectedContent;
            itemsPanel = null;

            base.OnDetaching();
        }

        private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (itemsPanel == null)
                return;

            switch (args.Action) {
                case NotifyCollectionChangedAction.Reset:
                    itemsPanel.Children.Clear();
                    break;

                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    if (args.OldItems != null) {
                        var generator = AssociatedObject.ItemContainerGenerator;
                        foreach (var oldItem in args.OldItems) {
                            var tabItem = oldItem as TabItem;
                            if (tabItem == null)
                                tabItem = generator.ContainerFromItem(oldItem) as TabItem;
                            var presenter = FindTabContentPresenter(tabItem);
                            if (presenter != null)
                                itemsPanel.Children.Remove(presenter);
                        }
                    }

                    // Ignore new items because we don't want to create visuals
                    // that aren't being shown.

                    UpdateSelectedContent();
                    break;
            }
        }

        private void OnSelectionChanged(object s, SelectionChangedEventArgs e)
        {
            UpdateSelectedContent();
        }

        private void OnGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (AssociatedObject.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                UpdateSelectedContent();
        }

        private void UpdateSelectedContent()
        {
            UpdateContentHost();

            foreach (ContentPresenter child in itemsPanel.Children)
                child.Visibility = Visibility.Collapsed;

            if (AssociatedObject.SelectedIndex >= 0) {
                TabItem selectedTabItem = GetSelectedTabItem();
                if (selectedTabItem != null)
                    GetOrCreateTabContentPresenter(selectedTabItem).Visibility = Visibility.Visible;
            }
        }

        private void UpdateContentHost()
        {
            if (ContentHost != null) {
                ContentHost.Content = itemsPanel;
                ContentHost.ContentSource = null;
                ContentHost.ContentStringFormat = null;
                ContentHost.ContentTemplateSelector = null;
            }
        }

        private ContentPresenter GetOrCreateTabContentPresenter(TabItem item)
        {
            if (item == null)
                return null;

            var presenter = FindTabContentPresenter(item);
            if (presenter != null)
                return presenter;

            presenter = new ContentPresenter {
                Content = item.Content,
                ContentTemplate = AssociatedObject.SelectedContentTemplate,
                ContentTemplateSelector = AssociatedObject.SelectedContentTemplateSelector,
                ContentStringFormat = AssociatedObject.SelectedContentStringFormat,
                Visibility = Visibility.Collapsed
            };

            itemsPanel.Children.Add(presenter);
            return presenter;
        }

        private ContentPresenter FindTabContentPresenter(TabItem tabItem)
        {
            if (itemsPanel == null || tabItem == null)
                return null;

            return itemsPanel.Children.Cast<ContentPresenter>().FirstOrDefault(
                x => x.Content == tabItem.Content);
        }

        private TabItem GetSelectedTabItem()
        {
            var selectedItem = AssociatedObject.SelectedItem;
            if (selectedItem == null)
                return null;

            var tabItem = selectedItem as TabItem;
            if (tabItem != null)
                return tabItem;

            var generator = AssociatedObject.ItemContainerGenerator;

            tabItem = generator.ContainerFromIndex(AssociatedObject.SelectedIndex) as TabItem;
            if (tabItem != null && Equals(selectedItem, generator.ItemFromContainer(tabItem)))
                return tabItem;

            return generator.ContainerFromItem(selectedItem) as TabItem;
        }
    }
}
