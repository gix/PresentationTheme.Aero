namespace SampleLib
{
    using System.Windows;
    using System.Windows.Controls;

    public class SampleControl : Control
    {
        static SampleControl()
        {
            var forType = typeof(SampleControl);
            DefaultStyleKeyProperty.OverrideMetadata(
                forType, new FrameworkPropertyMetadata(forType));
        }
    }
}
