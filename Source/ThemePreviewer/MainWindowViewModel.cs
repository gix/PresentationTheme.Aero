namespace ThemePreviewer
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Microsoft.Win32;
    using PresentationTheme.Aero.Win10;
    using PresentationTheme.Aero.Win7;
    using PresentationTheme.Aero.Win8;
    using PresentationTheme.AeroLite.Win10;
    using Samples;
    using StyleCore.Native;

    public class MainWindowViewModel : ViewModel
    {
        private string title = "Theme Previewer";
        private bool isEnabled = true;
        private object currentPage;
        private Theme currentTheme;
        private ProgressInfo taskProgress;
        private double scale = 1;
        private string nativeThemeName;
        private string wpfThemeName;

        public MainWindowViewModel()
        {
            ChangeThemeCommand = new DelegateCommand<Theme>(t => CurrentTheme = t);
            RestartWithThemeCommand = new DelegateCommand<Theme>(App.Current.RestartWithDifferentTheme);

            Themes = new ObservableCollection<Theme>();
            Themes.Add(new Theme("OS Default", null));
            Themes.Add(new Theme("Aero (Windows 7)", AeroWin7Theme.ResourceUri));
            Themes.Add(new Theme("Aero (Windows 8)", AeroWin8Theme.ResourceUri));
            Themes.Add(new Theme("Aero (Windows 10)", AeroWin10Theme.ResourceUri));
            Themes.Add(new Theme("Aero Lite (Windows 10)", AeroLiteWin10Theme.ResourceUri));
            Themes.Add(new Theme("Built-in Classic", BuiltinThemes.ClassicUri));
            Themes.Add(new Theme("Built-in Aero", BuiltinThemes.AeroUri));
            Themes.Add(new Theme("Built-in Aero 2", BuiltinThemes.Aero2Uri));
            Themes.Add(new Theme("Built-in Royale", BuiltinThemes.RoyaleUri));
            Themes.CollectionChanged += (s, e) => UpdateMenu();

            UpdateMenu();

            Pages.Add(ControlComparisonViewModel.Create<TextBoxSampleNative, TextBoxSampleWpf>("TextBox"));
            Pages.Add(ControlComparisonViewModel.Create<ButtonSampleNative, ButtonSampleWpf>("Button"));
            Pages.Add(ControlComparisonViewModel.Create<RadioCheckSampleNative, RadioCheckSampleWpf>("Radio/Check"));
            Pages.Add(ControlComparisonViewModel.Create<MenuSampleNative, MenuSampleWpf>("Menu"));
            Pages.Add(ControlComparisonViewModel.Create<ScrollBarSampleNative, ScrollBarSampleWpf>("ScrollBar"));
            Pages.Add(ControlComparisonViewModel.Create<ComboBoxSampleNative, ComboBoxSampleWpf>("ComboBox"));
            Pages.Add(ControlComparisonViewModel.Create<ListBoxSampleNative, ListBoxSampleWpf>("ListBox"));
            Pages.Add(ControlComparisonViewModel.Create<ListViewSampleNative, ListViewSampleWpf>("ListView"));
            Pages.Add(ControlComparisonViewModel.Create<TreeViewSampleNative, TreeViewSampleWpf>("TreeView"));
            Pages.Add(ControlComparisonViewModel.Create<DataGridSampleNative, DataGridSampleWpf>("DataGrid"));
            Pages.Add(ControlComparisonViewModel.Create<TabControlSampleNative, TabControlSampleWpf>("TabControl"));
            Pages.Add(ControlComparisonViewModel.Create<ProgressBarSampleNative, ProgressBarSampleWpf>("ProgressBar"));
            Pages.Add(ControlComparisonViewModel.Create<TrackbarSampleNative, TrackbarSampleWpf>("Trackbar"));
            Pages.Add(new ColorList());
            CurrentPage = Pages[0];

            NativeThemeName = "Native: OS";
            WpfThemeName = "WPF: Default";
        }

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public bool IsEnabled
        {
            get => isEnabled;
            set => SetProperty(ref isEnabled, value);
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
            var themeMenu = new MenuItem { Header = "_Theme" };
            themeMenu.Items.Clear();

            foreach (var theme in Themes) {
                var item = new MenuItem {
                    Header = theme.Name,
                    IsChecked = CurrentTheme == theme,
                    Command = RestartWithThemeCommand,
                    CommandParameter = theme
                };
                themeMenu.Items.Add(item);
            }

            themeMenu.Items.Add(new Separator());

            {
                themeMenu.Items.Add(new MenuItem {
                    Header = "Override native theme…",
                    Command = new DelegateCommand<bool>(OverrideNativeTheme),
                    CommandParameter = false
                });

                themeMenu.Items.Add(new MenuItem {
                    Header = "Override native theme (High Contrast)…",
                    Command = new DelegateCommand<bool>(OverrideNativeTheme),
                    CommandParameter = true
                });

                themeMenu.Items.Add(new MenuItem {
                    Header = "Remove native theme override",
                    Command = new DelegateCommand(RemoveNativeThemeOverride)
                });

                themeMenu.Items.Add(new MenuItem {
                    Header = "Broadcast theme change",
                    Command = new DelegateCommand(UxThemeExNativeMethods.UxBroadcastThemeChange)
                });
            }

            themeMenu.Items.Add(new Separator());

            themeMenu.Items.Add(new MenuItem {
                Header = "Exit",
                Command = new DelegateCommand(() => Application.Current.Shutdown())
            });

            MenuItems.Clear();
            MenuItems.Add(themeMenu);
        }

        private async void OverrideNativeTheme(bool highContrast = false)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "MSStyles (*.msstyles)|*.msstyles|All Files (*.*)|*.*";
            dialog.CheckFileExists = true;

            if (dialog.ShowDialog(App.Current.MainWindow) != true)
                return;

            await OverrideNativeTheme(dialog.FileName, highContrast);
        }

        public async Task OverrideNativeTheme(string themePath, bool highContrast)
        {
            if (await App.Current.OverrideNativeTheme(themePath, highContrast))
                NativeThemeName = $"Native: {Path.GetFileName(themePath)}";
        }

        public async void RemoveNativeThemeOverride()
        {
            if (await App.Current.RemoveNativeThemeOverride())
                NativeThemeName = "Native: OS";
        }

        public ObservableCollection<Theme> Themes { get; }
        public ObservableCollection<object> Pages { get; }
            = new ObservableCollection<object>();
        public ObservableCollection<object> MenuItems { get; } =
            new ObservableCollection<object>();

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
                    WpfThemeName = $"WPF: {value.Name}";
                    Title = $"Theme Previewer - {value.Name}";
                }
            }
        }

        public ProgressInfo TaskProgress
        {
            get => taskProgress;
            set => SetProperty(ref taskProgress, value);
        }

        public event EventHandler ThemeChanged;

        public async Task<bool> RunExclusive(
            Func<ProgressInfo, Task> action, string failureMessage)
        {
            try {
                IsEnabled = false;
                TaskProgress = new ProgressInfo();
                await action(TaskProgress);
                return true;
            } catch (Exception ex) {
                ShowError(ex, failureMessage);
                return false;
            } finally {
                IsEnabled = true;
                TaskProgress = null;
            }
        }

        private void ShowError(Exception exception, string caption)
        {
            ShowError(exception.Message, caption);
        }

        private void ShowError(HResult hr, string caption)
        {
            ShowError(new Win32Exception((int)hr).Message, caption);
        }

        private void ShowError(string message, string caption)
        {
            MessageBox.Show(
                MainWindow, message, caption, MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private Window MainWindow => App.Current.MainWindow;
    }
}
