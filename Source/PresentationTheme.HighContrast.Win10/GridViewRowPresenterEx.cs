namespace PresentationTheme.HighContrast.Win10
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public class GridViewRowPresenterEx : GridViewRowPresenter
    {
        private const double PaddingHeaderMinWidth = 2;

        protected override Size MeasureOverride(Size constraint)
        {
            var size = base.MeasureOverride(constraint);
            size.Width = Math.Max(size.Width - PaddingHeaderMinWidth, 0);
            return size;
        }
    }
}
