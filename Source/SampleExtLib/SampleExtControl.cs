namespace SampleExtLib
{
    using System.Windows;
    using System.Windows.Controls;

    public class SampleExtControl : Control
    {
        static SampleExtControl()
        {
            var forType = typeof(SampleExtControl);
            DefaultStyleKeyProperty.OverrideMetadata(
                forType, new FrameworkPropertyMetadata(forType));
        }
    }
}
