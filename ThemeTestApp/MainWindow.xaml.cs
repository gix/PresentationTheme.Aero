namespace ThemeTestApp
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Samples;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private object currentPage;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Pages = new ObservableCollection<object>();
            Pages.Add(Sample.Create<SysListViewSample, WpfListViewSample>("ListView"));
            Pages.Add(Sample.Create<SysTreeViewSample, WpfTreeViewSample>("TreeView"));
            Pages.Add(Sample.Create<SysDataGridSample, WpfDataGridSample>("DataGrid"));
            Pages.Add(Sample.Create<SysMenuSample, WpfMenuSample>("Menu"));
            Pages.Add(new ColorList());
            CurrentPage = Pages[3];
        }

        public ObservableCollection<object> Pages { get; private set; }

        public object CurrentPage
        {
            get { return currentPage; }
            set
            {
                if (currentPage != value) {
                    currentPage = value;
                    RaisePropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
