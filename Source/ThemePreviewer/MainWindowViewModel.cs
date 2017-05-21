namespace ThemePreviewer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using Microsoft.Win32;
    using Samples;
    using StyleCore.Native;

    public class MainWindowViewModel : ViewModel
    {
        private readonly ThemeInfoProvider themeInfoProvider;
        private readonly AsyncDelegateCommand<ThemeInfoPair> changeThemeCommand;
        private readonly AsyncDelegateCommand<WpfThemeInfo> changeWpfThemeCommand;
        private readonly UxThemeOverride uxThemeOverride = new UxThemeOverride();

        private string title = "Theme Previewer";
        private bool isEnabled = true;
        private object currentPage;
        private WpfThemeInfo currentWpfTheme;
        private NativeThemeInfo currentNativeTheme;
        private ProgressInfo taskProgress;
        private double scale = 1;
        private string nativeThemeName;
        private string wpfThemeName;

        public MainWindowViewModel(ThemeInfoProvider themeInfoProvider)
        {
            this.themeInfoProvider = themeInfoProvider;

            changeThemeCommand = new AsyncDelegateCommand<ThemeInfoPair>(ChangeTheme);
            changeWpfThemeCommand = new AsyncDelegateCommand<WpfThemeInfo>(ChangeWpfTheme);

            foreach (var theme in themeInfoProvider.Themes)
                Themes.Add(theme);
            foreach (var theme in themeInfoProvider.WpfThemes)
                WpfThemes.Add(theme);
            foreach (var theme in themeInfoProvider.NativeThemes)
                NativeThemes.Add(theme);

            Themes.CollectionChanged += (s, e) => UpdateMenu();
            WpfThemes.CollectionChanged += (s, e) => UpdateMenu();
            NativeThemes.CollectionChanged += (s, e) => UpdateMenu();
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
            Pages.Add(ControlComparisonViewModel.Create<GroupBoxSampleNative, GroupBoxSampleWpf>("GroupBox"));
            Pages.Add(new ColorList());
            CurrentPage = Pages[0];

            NativeThemeName = "Native: Default";
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

        public ObservableCollection<ThemeInfoPair> Themes { get; } =
            new ObservableCollection<ThemeInfoPair>();

        public ObservableCollection<WpfThemeInfo> WpfThemes { get; } =
            new ObservableCollection<WpfThemeInfo>();

        public ObservableCollection<NativeThemeInfo> NativeThemes { get; } =
            new ObservableCollection<NativeThemeInfo>();

        public ObservableCollection<object> Pages { get; } =
            new ObservableCollection<object>();

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

        public WpfThemeInfo CurrentWpfTheme
        {
            get { return currentWpfTheme; }
            set
            {
                if (SetProperty(ref currentWpfTheme, value)) {
                    UpdateMenu();
                    WpfThemeName = "WPF: " + (value?.Name ?? "Default");
                }
            }
        }

        public NativeThemeInfo CurrentNativeTheme
        {
            get { return currentNativeTheme; }
            private set
            {
                if (SetProperty(ref currentNativeTheme, value)) {
                    UpdateMenu();
                    NativeThemeName = "Native: " + (value?.GetFullName() ?? "Default");
                }
            }
        }

        public ProgressInfo TaskProgress
        {
            get => taskProgress;
            private set => SetProperty(ref taskProgress, value);
        }

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
                Application.Current.MainWindow, message, caption,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void UpdateMenu()
        {
            var themeMenu = new MenuItem { Header = "_Theme" };
            themeMenu.Items.Clear();

            foreach (var theme in Themes) {
                var item = new MenuItem {
                    Header = theme.Name,
                    IsChecked = CurrentWpfTheme == theme.WpfTheme && CurrentNativeTheme == theme.NativeTheme,
                    Command = changeThemeCommand,
                    CommandParameter = theme
                };
                themeMenu.Items.Add(item);
            }

            if (Themes.Count > 0)
                themeMenu.Items.Add(new Separator());

            {
                var subMenu = new MenuItem {
                    Header = CurrentWpfTheme != null ? $"WPF: {CurrentWpfTheme.Name}" : "WPF (Default)"
                };
                themeMenu.Items.Add(subMenu);

                subMenu.Items.Add(new MenuItem {
                    Header = "Default",
                    IsChecked = CurrentWpfTheme == null,
                    Command = changeWpfThemeCommand,
                    CommandParameter = null
                });
                subMenu.Items.Add(new Separator());

                foreach (var theme in WpfThemes) {
                    subMenu.Items.Add(new MenuItem {
                        Header = theme.Name,
                        IsChecked = CurrentWpfTheme == theme,
                        Command = changeWpfThemeCommand,
                        CommandParameter = theme
                    });
                }
            }

            {
                var subMenu = new MenuItem {
                    Header = CurrentNativeTheme != null ? $"Native: {CurrentNativeTheme.Name}" : "Native (Default)"
                };
                themeMenu.Items.Add(subMenu);

                subMenu.Items.Add(new MenuItem {
                    Header = "Default",
                    IsChecked = CurrentNativeTheme == null,
                    Command = new AsyncDelegateCommand(RemoveNativeThemeOverride)
                });
                subMenu.Items.Add(new Separator());

                foreach (var theme in NativeThemes) {
                    subMenu.Items.Add(new MenuItem {
                        Header = theme.GetFullName(),
                        IsChecked = theme == CurrentNativeTheme,
                        Command = new AsyncDelegateCommand(
                            async () => await OverrideNativeTheme(theme))
                    });
                }
            }

            themeMenu.Items.Add(new Separator());

            {
                var menuItem = new MenuItem();
                menuItem.Header = "Override native theme…";
                menuItem.CommandParameter = false;
                menuItem.Command = new AsyncDelegateCommand<bool>(OverrideNativeTheme);
                themeMenu.Items.Add(menuItem);

                themeMenu.Items.Add(new MenuItem {
                    Header = "Override native theme (High Contrast)…",
                    Command = new AsyncDelegateCommand<bool>(OverrideNativeTheme),
                    CommandParameter = true
                });

                themeMenu.Items.Add(new MenuItem {
                    Header = "Remove native theme override",
                    Command = new AsyncDelegateCommand(RemoveNativeThemeOverride)
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

        public async Task<bool> ChangeTheme(ThemeInfoPair theme)
        {
            return await RunExclusive(async progress => {
                progress.TaskName = "Overriding native theme…";
                await uxThemeOverride.SetThemeAsync(
                    theme.NativeTheme.Path.FullName, theme.NativeTheme.LoadParams);
                CurrentNativeTheme = theme.NativeTheme;
                await ChangeWpfTheme(theme.WpfTheme);
            }, "Failed to override native theme");
        }

        public async Task ChangeWpfTheme(WpfThemeInfo theme)
        {
            await uxThemeOverride.SetPresentationFrameworkTheme(
                Dispatcher.CurrentDispatcher, theme?.ResourceUri);
            CurrentWpfTheme = theme;
        }

        private async Task OverrideNativeTheme(bool highContrast = false)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "MSStyles (*.msstyles)|*.msstyles|All Files (*.*)|*.*";
            dialog.CheckFileExists = true;

            if (dialog.ShowDialog(Application.Current.MainWindow) != true)
                return;

            await OverrideNativeTheme(dialog.FileName, new UxThemeLoadParams {
                IsHighContrast = highContrast
            });
        }

        public async Task<bool> OverrideNativeTheme(NativeThemeInfo theme)
        {
            return await OverrideNativeTheme(theme.Path.FullName, theme.LoadParams);
        }

        public async Task<bool> OverrideNativeTheme(
            string themePath, UxThemeLoadParams loadParams = null)
        {
            return await RunExclusive(async progress => {
                progress.TaskName = "Overriding native theme…";
                await uxThemeOverride.SetThemeAsync(themePath, loadParams);
                CurrentNativeTheme =
                    NativeThemes.FirstOrDefault(x => x.Matches(themePath, loadParams)) ??
                    NativeThemeInfo.FromPath(themePath, loadParams);
            }, "Failed to override native theme");
        }

        public async Task<bool> RemoveNativeThemeOverride()
        {
            return await RunExclusive(async progress => {
                progress.TaskName = "Restoring native theme…";
                await uxThemeOverride.SetThemeAsync(null);
                CurrentNativeTheme = null;
            }, "Failed to restore native theme");
        }

        public void AppendCommandLineArgs(ICollection<string> args)
        {
            var tabIndex = Pages.IndexOf(CurrentPage);
            if (tabIndex != -1)
                args.Add($"-tab:{tabIndex}");
            if (uxThemeOverride.CurrentOverride != null)
                args.Add($"-nativetheme:{uxThemeOverride.CurrentOverride}");
        }
    }
}
