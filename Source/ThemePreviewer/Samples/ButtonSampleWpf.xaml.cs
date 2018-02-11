namespace ThemePreviewer.Samples
{
    using System.Windows;
    using System.Windows.Controls;

    public partial class ButtonSampleWpf
    {
        public ButtonSampleWpf()
        {
            InitializeComponent();

            normalButton.Click += (s, e) => VisualStateManager.GoToState(customButton, "Normal", useTransitionsFlag.IsChecked == true);
            mouseOverButton.Click += (s, e) => VisualStateManager.GoToState(customButton, "MouseOver", useTransitionsFlag.IsChecked == true);
            pressedButton.Click += (s, e) => VisualStateManager.GoToState(customButton, "Pressed", useTransitionsFlag.IsChecked == true);
            disabledButton.Click += (s, e) => VisualStateManager.GoToState(customButton, "Disabled", useTransitionsFlag.IsChecked == true);

            focusButton.Click += (s, e) => VisualStateManager.GoToState(customButton, "Focused", useTransitionsFlag.IsChecked == true);
            unfocusButton.Click += (s, e) => VisualStateManager.GoToState(customButton, "Unfocused", useTransitionsFlag.IsChecked == true);

            defaulted.Click += (s, e) => status.Text = ((Button)s).Content as string;
            normalButton.Click += (s, e) => status.Text = ((Button)s).Content as string;
            mouseOverButton.Click += (s, e) => status.Text = ((Button)s).Content as string;
            pressedButton.Click += (s, e) => status.Text = ((Button)s).Content as string;
            disabledButton.Click += (s, e) => status.Text = ((Button)s).Content as string;
            focusButton.Click += (s, e) => status.Text = ((Button)s).Content as string;
            unfocusButton.Click += (s, e) => status.Text = ((Button)s).Content as string;

            hotChrome.SetForceIsMouseOver(true);
            pressedChrome.SetForceIsPressed(true);
        }
    }
}
