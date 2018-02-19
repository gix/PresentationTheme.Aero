namespace ThemeBrowser
{
    using System;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;

    public class FrameworkElementAdorner : Adorner
    {
        private readonly FrameworkElement child;

        public FrameworkElementAdorner(UIElement adornedElement, FrameworkElement child)
            : base(adornedElement)
        {
            this.child = child ?? throw new ArgumentNullException(nameof(child));
            AddVisualChild(child);
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            return child;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            child.Measure(constraint);
            return constraint;
        }

        protected override Size ArrangeOverride(Size size)
        {
            Size finalSize = base.ArrangeOverride(size);
            child.Arrange(new Rect(new Point(), finalSize));
            return finalSize;
        }
    }
}
