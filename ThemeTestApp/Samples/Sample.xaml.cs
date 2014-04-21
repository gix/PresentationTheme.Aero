namespace ThemeTestApp.Samples
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms.Integration;

    /// <summary>
    /// Interaction logic for Sample.xaml
    /// </summary>
    public partial class Sample : UserControl
    {
        /// <summary>
        ///   Identifies the <see cref="Label"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                "Label",
                typeof(string),
                typeof(Sample),
                new PropertyMetadata(null));

        /// <summary>
        ///   Identifies the <see cref="WpfSample"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WpfSampleProperty =
            DependencyProperty.Register(
                "WpfSample",
                typeof(object),
                typeof(Sample),
                new PropertyMetadata(null, OnWpfSampleChanged));

        /// <summary>
        ///   Identifies the <see cref="SysSample"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SysSampleProperty =
            DependencyProperty.Register(
                "SysSample",
                typeof(System.Windows.Forms.Control),
                typeof(Sample),
                new PropertyMetadata(null, OnSysSampleChanged));

        public Sample(string name)
        {
            InitializeComponent();
            DataContext = this;
            Label = name;
            Options = new ObservableCollection<Option>();
        }

        /// <summary>
        ///   Gets or sets the Label of the <see cref="Sample"/>.
        /// </summary>
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public ObservableCollection<Option> Options { get; private set; }

        /// <summary>
        ///   Gets or sets the WpfSample of the <see cref="Sample"/>.
        /// </summary>
        public object WpfSample
        {
            get { return (object)GetValue(WpfSampleProperty); }
            set { SetValue(WpfSampleProperty, value); }
        }

        /// <summary>
        ///   Gets or sets the SysSample of the <see cref="Sample"/>.
        /// </summary>
        public System.Windows.Forms.Control SysSample
        {
            get { return (System.Windows.Forms.Control)GetValue(SysSampleProperty); }
            set { SetValue(SysSampleProperty, value); }
        }

        public static Sample Create<TSys, TWpf>(string name)
            where TWpf : new()
            where TSys : System.Windows.Forms.Control, new()
        {
            return new Sample(name) {
                SysSample = new TSys(),
                WpfSample = new TWpf()
            };
        }

        private static void OnWpfSampleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sample = (Sample)d;
            var oldValue = e.OldValue;
            var newValue = e.NewValue;
            sample.OnWpfSampleChanged(oldValue, newValue);
        }

        private static void OnSysSampleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sample = (Sample)d;
            var oldValue = (System.Windows.Forms.Control)e.OldValue;
            var newValue = (System.Windows.Forms.Control)e.NewValue;
            sample.OnSysSampleChanged(oldValue, newValue);
        }

        private void OnSysSampleChanged(
            System.Windows.Forms.Control oldValue, System.Windows.Forms.Control newValue)
        {
            WinFormsHost.Child = newValue;
            UpdateOptions(oldValue, newValue);
        }

        private void OnWpfSampleChanged(object oldValue, object newValue)
        {
            UpdateOptions(oldValue, newValue);
        }

        private void UpdateOptions(object oldValue, object newValue)
        {
            var oldOptionControl = oldValue as IOptionControl;
            if (oldOptionControl != null) {
                foreach (var option in oldOptionControl.Options) {
                    option.PropertyChanged -= OnOptionChanged;
                    Options.Remove(option);
                }
            }

            var newOptionControl = newValue as IOptionControl;
            if (newOptionControl != null) {
                foreach (var option in newOptionControl.Options) {
                    option.PropertyChanged += OnOptionChanged;
                    Options.Add(option);
                }
            }
        }

        private void OnOptionChanged(object sender, PropertyChangedEventArgs e)
        {
        }
    }
}
