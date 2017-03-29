namespace ThemePreviewer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using PresentationTheme.Aero.Win10;
    using PresentationTheme.Aero.Win7;
    using PresentationTheme.Aero.Win8;
    using Samples;
    using StyleCore.Native;
    using Application = System.Windows.Application;
    using MenuItem = System.Windows.Controls.MenuItem;
    using MessageBox = System.Windows.MessageBox;
    using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

    public partial class MainWindow : INotifyPropertyChanged
    {
        private object currentPage;
        private Theme currentTheme;
        private double scale = 1;

        private string nativeThemeName;
        private string wpfThemeName;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            NativeThemeName = "Native";
            WpfThemeName = "WPF";
            ChangeThemeCommand = new DelegateCommand<Theme>(t => CurrentTheme = t);
            RestartWithThemeCommand = new DelegateCommand<Theme>(App.Current.RestartWithDifferentTheme);

            Themes = new ObservableCollection<Theme>();
            Themes.Add(new Theme("OS Default", null));
            Themes.Add(new Theme("Aero (Windows 7)", AeroWin7Theme.ResourceUri));
            Themes.Add(new Theme("Aero (Windows 8)", AeroWin8Theme.ResourceUri));
            Themes.Add(new Theme("Aero (Windows 10)", AeroWin10Theme.ResourceUri));
            Themes.Add(new Theme("Built-in Classic", BuiltinThemes.ClassicUri));
            Themes.Add(new Theme("Built-in Aero", BuiltinThemes.AeroUri));
            Themes.Add(new Theme("Built-in Aero 2", BuiltinThemes.Aero2Uri));
            Themes.Add(new Theme("Built-in Royale", BuiltinThemes.RoyaleUri));
            Themes.CollectionChanged += (s, e) => UpdateMenu();

            UpdateMenu();

            Pages = new ObservableCollection<object>();
            Pages.Add(ControlComparison.Create<ButtonSampleNative, ButtonSampleWpf>("Button"));
            Pages.Add(ControlComparison.Create<RadioCheckSampleNative, RadioCheckSampleWpf>("Radio/Check"));
            Pages.Add(ControlComparison.Create<MenuSampleNative, MenuSampleWpf>("Menu"));
            Pages.Add(ControlComparison.Create<ScrollBarSampleNative, ScrollBarSampleWpf>("ScrollBar"));
            Pages.Add(ControlComparison.Create<ComboBoxSampleNative, ComboBoxSampleWpf>("ComboBox"));
            Pages.Add(ControlComparison.Create<ListBoxSampleNative, ListBoxSampleWpf>("ListBox"));
            Pages.Add(ControlComparison.Create<ListViewSampleNative, ListViewSampleWpf>("ListView"));
            Pages.Add(ControlComparison.Create<TreeViewSampleNative, TreeViewSampleWpf>("TreeView"));
            Pages.Add(ControlComparison.Create<DataGridSampleNative, DataGridSampleWpf>("DataGrid"));
            Pages.Add(ControlComparison.Create<TabControlSampleNative, TabControlSampleWpf>("TabControl"));
            Pages.Add(ControlComparison.Create<ProgressBarSampleNative, ProgressBarSampleWpf>("ProgressBar"));
            Pages.Add(ControlComparison.Create<TrackbarSampleNative, TrackbarSampleWpf>("Trackbar"));
            Pages.Add(new ColorList());
            CurrentPage = Pages[0];
        }

        public string NativeThemeName
        {
            get => nativeThemeName;
            set => SetProperty(ref nativeThemeName, value);
        }

        public string WpfThemeName
        {
            get => wpfThemeName;
            set => SetProperty(ref wpfThemeName, value);
        }

        public ICommand ChangeThemeCommand { get; }
        public ICommand RestartWithThemeCommand { get; }

        private void UpdateMenu()
        {
            themeItem.Items.Clear();

            foreach (var theme in Themes) {
                var item = new MenuItem {
                    Header = theme.Name,
                    IsChecked = CurrentTheme == theme,
                    Command = RestartWithThemeCommand,
                    CommandParameter = theme
                };
                themeItem.Items.Add(item);
            }

            themeItem.Items.Add(new Separator());

            {
                var item = new MenuItem {
                    Header = "Override native theme…",
                    Command = new DelegateCommand(OverrideNativeTheme)
                };
                themeItem.Items.Add(item);
            }

            {
                var item = new MenuItem {
                    Header = "Remove native theme override",
                    Command = new DelegateCommand(RemoveNativeThemeOverride)
                };
                themeItem.Items.Add(item);
            }

            {
                var item = new MenuItem {
                    Header = "Broadcast theme change",
                    Command = new DelegateCommand(UxThemeExNativeMethods.UxBroadcastThemeChange)
                };
                themeItem.Items.Add(item);
            }

            {
                var item = new MenuItem {
                    Header = "Exit",
                    Command = new DelegateCommand(() => Application.Current.Shutdown())
                };
                themeItem.Items.Add(item);
            }
        }

        private void OverrideNativeTheme()
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "MSStyles (*.msstyles)|*.msstyles|All Files (*.*)|*.*";
            dialog.CheckFileExists = true;

            if (dialog.ShowDialog(this) != true)
                return;

            Run(async () => await App.Current.OverrideNativeTheme(dialog.FileName));
        }

        private void Run(Action action)
        {
            try {
                action();
            } catch (Exception ex) {
                MessageBox.Show(App.Current.MainWindow, ex.Message);
            }
        }

        private void RemoveNativeThemeOverride()
        {
            Run(async () => await App.Current.OverrideNativeTheme(null));
        }

        public ObservableCollection<Theme> Themes { get; }
        public ObservableCollection<object> Pages { get; }

        public double Scale
        {
            get => scale;
            set => SetProperty(ref scale, Math.Max(1, Math.Min(4, value)));
        }

        public object CurrentPage
        {
            get => currentPage;
            set => SetProperty(ref currentPage, value);
        }

        public Theme CurrentTheme
        {
            get { return currentTheme; }
            set
            {
                if (SetProperty(ref currentTheme, value)) {
                    UpdateMenu();
                    ThemeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(
            ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            field = newValue;
            RaisePropertyChanged(propertyName);
            return true;
        }

        public event EventHandler ThemeChanged;

        private void OnTextFormattingFlagClicked(object sender, RoutedEventArgs e)
        {
            var mode = textFormattingFlag.IsChecked == true ?
                TextFormattingMode.Display : TextFormattingMode.Ideal;

            TextOptions.SetTextFormattingMode(this, mode);
        }

        private void OnAnimationFlagClicked(object sender, RoutedEventArgs e)
        {
            AeroWin10Theme.UseAnimationsOverride = animationFlag.IsChecked;
        }
    }
}
