namespace ThemePreviewer
{
    using System;
    using System.Windows;

    public class ControlComparisonViewModel : ViewModel
    {
        private string label;
        private Type nativeSampleType;
        private Type wpfSampleType;

        public string Label
        {
            get => label;
            set => SetProperty(ref label, value);
        }

        public Type NativeSampleType
        {
            get => nativeSampleType;
            set => SetProperty(ref nativeSampleType, value);
        }

        public Type WpfSampleType
        {
            get => wpfSampleType;
            set => SetProperty(ref wpfSampleType, value);
        }

        public object NativeSample => NativeSampleType;

        public object WpfSample => WpfSampleType;

        public static ControlComparisonViewModel Create<TNative, TWpf>(string name)
            where TWpf : FrameworkElement, new()
            where TNative : System.Windows.Forms.Control, new()
        {
            return new ControlComparisonViewModel {
                Label = name,
                NativeSampleType = typeof(TNative),
                WpfSampleType = typeof(TWpf)
            };
        }
    }
}
