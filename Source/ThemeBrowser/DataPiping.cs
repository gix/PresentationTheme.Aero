namespace ThemeBrowser
{
    using System.Windows;

    public static class DataPiping
    {
        public static readonly DependencyProperty DataPipesProperty =
            DependencyProperty.RegisterAttached(
                "DataPipes",
                typeof(DataPipeCollection),
                typeof(DataPiping),
                new UIPropertyMetadata(null));

        public static void SetDataPipes(DependencyObject d, DataPipeCollection value)
        {
            d.SetValue(DataPipesProperty, value);
        }

        public static DataPipeCollection GetDataPipes(DependencyObject o)
        {
            return (DataPipeCollection)o.GetValue(DataPipesProperty);
        }
    }

    public class DataPipeCollection : FreezableCollection<DataPipe>
    {
    }

    public class DataPipe : Freezable
    {
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                nameof(Source),
                typeof(object),
                typeof(DataPipe),
                new FrameworkPropertyMetadata(null, OnSourceChanged));

        public object Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DataPipe)d).OnSourceChanged(e);
        }

        protected virtual void OnSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            Target = e.NewValue;
        }

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register(
                nameof(Target),
                typeof(object),
                typeof(DataPipe),
                new FrameworkPropertyMetadata(null));

        public object Target
        {
            get => GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new DataPipe();
        }
    }
}
