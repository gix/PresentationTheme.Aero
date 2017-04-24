namespace ThemePreviewer
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Interactivity;

    public class ShowProgressAdornerBehavior : Behavior<Window>
    {
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(
                nameof(Progress),
                typeof(ProgressInfo),
                typeof(ShowProgressAdornerBehavior),
                new PropertyMetadata(null, OnProgressChanged));

        private AdornerLayer adornerLayer;
        private FrameworkElementAdorner adorner;

        public ProgressInfo Progress
        {
            get => (ProgressInfo)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        private static void OnProgressChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (ShowProgressAdornerBehavior)d;
            source.OnProgressChanged();
        }

        private void OnProgressChanged()
        {
            if (Progress != null && AssociatedObject != null)
                AddAdorner();
            else
                RemoveAdorner();
        }

        private void AddAdorner()
        {
            var element = AssociatedObject.Content as UIElement;
            if (element == null)
                return;

            var content = new ContentControl();
            content.HorizontalAlignment = HorizontalAlignment.Stretch;
            content.VerticalAlignment = VerticalAlignment.Stretch;
            content.SetBinding(ContentControl.ContentProperty, new Binding(nameof(Progress)) {
                Source = this
            });
            adorner = new FrameworkElementAdorner(element, content);

            adornerLayer = AdornerLayer.GetAdornerLayer(element);
            adornerLayer.Add(adorner);
        }

        private void RemoveAdorner()
        {
            if (adorner != null) {
                adornerLayer.Remove(adorner);
                adorner = null;
                adornerLayer = null;
            }
        }
    }
}
