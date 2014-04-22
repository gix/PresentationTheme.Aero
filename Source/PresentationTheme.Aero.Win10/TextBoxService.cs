namespace PresentationTheme.Aero.Win10
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Threading;

    /// <summary>
    ///   Provides various extensions for <see cref="TextBox"/> controls.
    /// </summary>
    public static class TextBoxService
    {
        private static readonly Type TextBoxViewType =
            typeof(TextBox).Assembly.GetType("System.Windows.Controls.TextBoxView");

        private static DependencyPropertyDescriptor contentPropertyDescriptor;

        private static DependencyPropertyDescriptor templatePropertyDescriptor;

        private static DependencyPropertyDescriptor ContentPropertyDescriptor =>
            contentPropertyDescriptor ??
            (contentPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(
                ContentControl.ContentProperty, typeof(ScrollViewer)));

        private static DependencyPropertyDescriptor TemplatePropertyDescriptor =>
            templatePropertyDescriptor ??
            (templatePropertyDescriptor = DependencyPropertyDescriptor.FromProperty(
                Control.TemplateProperty, typeof(Control)));

        #region public attached Thickness ViewMargin
        /// <summary>
        ///   Identifies the ViewMargin dependency property.
        /// </summary>
        /// <remarks>
        ///   <see cref="TextBox"/> controls have a hardcoded margin of (0;2).
        ///   Setting this attached property on a <see cref="TextBox"/> allows
        ///   changing this margin.
        /// </remarks>
        public static readonly DependencyProperty ViewMarginProperty =
            DependencyProperty.RegisterAttached(
                "ViewMargin",
                typeof(Thickness?),
                typeof(TextBoxService),
                new PropertyMetadata(null, OnViewMarginChanged));

        /// <summary>
        ///   Gets the value of the attached <see cref="ViewMarginProperty"/>
        ///   for a specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="d">
        ///   The <see cref="DependencyObject"/> from which the property value
        ///   is read.
        /// </param>
        /// <returns>
        ///   The view margin for the <see cref="DependencyObject"/>.
        /// </returns>
        public static Thickness? GetViewMargin(DependencyObject d)
        {
            if (d == null)
                throw new ArgumentNullException(nameof(d));
            return (Thickness?)d.GetValue(ViewMarginProperty);
        }

        /// <summary>
        ///   Sets the value of the attached <see cref="ViewMarginProperty"/> to
        ///   a specified <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="d">
        ///   The <see cref="DependencyObject"/> to which the property is attached.
        /// </param>
        /// <param name="value">The view margin.</param>
        public static void SetViewMargin(DependencyObject d, Thickness? value)
        {
            if (d == null)
                throw new ArgumentNullException(nameof(d));
            d.SetValue(ViewMarginProperty, value);
        }

        private static void OnViewMarginChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var textBox = d as TextBox;
            if (textBox != null) {
                if (args.NewValue != null)
                    Attach(textBox);
                else
                    Detach(textBox);
            }
        }
        #endregion

        private static void Attach(TextBox textBox)
        {
            TemplatePropertyDescriptor.AddValueChanged(
                textBox, OnTextBoxTemplateChanged);
            ApplyMargin(textBox);
        }

        private static void Detach(TextBox textBox)
        {
            TemplatePropertyDescriptor.RemoveValueChanged(
                textBox, OnTextBoxTemplateChanged);
            textBox.Loaded -= OnTextBoxLoaded;
        }

        private static void OnTextBoxTemplateChanged(object sender, EventArgs e)
        {
            var textBox = (TextBox)sender;
            ApplyMargin(textBox);
        }

        private static void ApplyMargin(TextBox textBox)
        {
            if (textBox.IsLoaded)
                DoApplyMargin(textBox);
            else
                textBox.Loaded += OnTextBoxLoaded;
        }

        private static void OnTextBoxLoaded(object sender, RoutedEventArgs args)
        {
            var textBox = (TextBox)sender;
            textBox.Loaded -= OnTextBoxLoaded;
            DoApplyMargin(textBox);
        }

        private static void DoApplyMargin(TextBox textBox, bool mayDefer = true)
        {
            var contentHost = textBox.Template.FindName("PART_ContentHost", textBox) as ScrollViewer;
            if (contentHost != null) {
                if (contentHost.HasContent)
                    DoApplyMargin(contentHost);
                else
                    ContentPropertyDescriptor.AddValueChanged(
                        contentHost, OnContentHostContentChanged);
            } else if (mayDefer) {
                textBox.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => {
                    DoApplyMargin(textBox, false);
                }));
            }
        }

        private static void OnContentHostContentChanged(object sender, EventArgs e)
        {
            var contentHost = (ScrollViewer)sender;
            ContentPropertyDescriptor.RemoveValueChanged(
                contentHost, OnContentHostContentChanged);
            DoApplyMargin(contentHost);
        }

        private static void DoApplyMargin(ScrollViewer contentHost)
        {
            var textBox = contentHost.FindAncestor<TextBox>();
            var element = contentHost.Content as FrameworkElement;
            if (textBox != null && element != null && element.GetType() == TextBoxViewType) {
                var margin = GetViewMargin(textBox);
                if (margin != null)
                    element.Margin = margin.Value;
            }
        }
    }
}
