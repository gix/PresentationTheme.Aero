namespace ThemeBrowser
{
    using System.Windows;
    using System.Windows.Controls;

    public class SelectableTextBlock : TextBox
    {
        static SelectableTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SelectableTextBlock),
                new FrameworkPropertyMetadata(typeof(SelectableTextBlock)));
        }
    }
}
