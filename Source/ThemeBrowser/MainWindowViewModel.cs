﻿namespace StyleInspector
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Win32;
    using StyleCore;
    using StyleCore.Native;

    public class MainWindowViewModel : ViewModel
    {
        private readonly object mutex = new object();
        private ThemeFileViewModel themeFile;
        private ObservableCollection<ThemeRawProperty> allProperties;
        private List<TransitionDuration> transitionDurations;
        private object selectedItem;
        private object content;

        public MainWindowViewModel()
        {
            OpenCommand = new DelegateCommand(Open);
            CompareCommand = new DelegateCommand(Compare);
            ExitCommand = new DelegateCommand(Exit);
            CopyVisualStatesCommand = new AsyncDelegateCommand<object>(CopyVisualStates);
            SaveImageCommand = new AsyncDelegateCommand<ThemeBitmapViewModel>(OnSaveImage, CanSaveImage);
            SaveUnpremultipliedImageCommand = new AsyncDelegateCommand<ThemeBitmapViewModel>(OnSaveUnpremultipliedImage, CanSaveImage);
            TraceImageCommand = new AsyncDelegateCommand<ThemeBitmapViewModel>(TraceImage);
            PreviewImageCommand = new AsyncDelegateCommand<ThemeBitmapViewModel>(PreviewImage);
            AllProperties = new ObservableCollection<ThemeRawProperty>();
            BindingOperations.EnableCollectionSynchronization(AllProperties, mutex);
        }

        private Window MainWindow => Application.Current.MainWindow;

        private bool CanSaveImage(ThemeBitmapViewModel themeBitmap)
        {
            return themeBitmap != null;
        }

        private Task OnSaveImage(ThemeBitmapViewModel themeBitmap)
        {
            try {
                if (themeBitmap != null) {
                    var saveDialog = new SaveFileDialog {
                        FileName = $"{themeBitmap.ImageId}.png",
                        DefaultExt = ".png"
                    };
                    if (saveDialog.ShowDialog(MainWindow) != true)
                        return Task.CompletedTask;

                    using (var output = saveDialog.OpenFile())
                    using (var input = themeBitmap.OpenStream())
                        input.CopyTo(output);
                }
            } catch (Exception ex) {
                MessageBox.Show(MainWindow, "Failed to save", ex.Message);
            }

            return Task.CompletedTask;
        }

        private Task OnSaveUnpremultipliedImage(ThemeBitmapViewModel themeBitmap)
        {
            try {
                if (themeBitmap != null) {
                    var saveDialog = new SaveFileDialog {
                        FileName = $"{themeBitmap.ImageId}.png",
                        DefaultExt = ".png"
                    };
                    if (saveDialog.ShowDialog(MainWindow) != true)
                        return Task.CompletedTask;

                    using (var output = saveDialog.OpenFile())
                    using (var input = themeBitmap.OpenStream())
                        SaveUnpremultipliedImage(output, input);
                }
            } catch (Exception ex) {
                MessageBox.Show(MainWindow, "Failed to save", ex.Message);
            }

            return Task.CompletedTask;
        }

        private unsafe void SaveUnpremultipliedImage(Stream output, Stream input)
        {
            var source = new BitmapImage();
            source.BeginInit();
            source.StreamSource = input;
            source.EndInit();

            var image = new WriteableBitmap(source);

            image.Lock();
            try {
                var ptr = (byte*)image.BackBuffer.ToPointer();
                for (int h = 0; h < image.PixelHeight; ++h) {
                    var pixel = (uint*)ptr;
                    for (int w = 0; w < image.PixelWidth; ++w) {
                        *pixel = Unpremultiply(*pixel);
                        ++pixel;
                    }

                    ptr += image.BackBufferStride;
                }
            } finally {
                image.Unlock();
            }

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            encoder.Save(output);
        }

        private uint Unpremultiply(uint argb)
        {
            double a = ((argb >> 24) & 0xFF) / 255.0;
            double r = ((argb >> 16) & 0xFF) / 255.0;
            double g = ((argb >> 8) & 0xFF) / 255.0;
            double b = ((argb >> 0) & 0xFF) / 255.0;

            r /= a;
            g /= a;
            b /= a;

            var ba = (byte)Math.Round(a * 255);
            var br = (byte)Math.Round(r * 255);
            var bg = (byte)Math.Round(g * 255);
            var bb = (byte)Math.Round(b * 255);
            return bb | (uint)(bg << 8) | (uint)(br << 16) | (uint)(ba << 24);
        }

        private Task TraceImage(ThemeBitmapViewModel bitmap)
        {
            if (bitmap == null)
                return Task.CompletedTask;

            var dialog = new TraceImageDialog(bitmap.Bitmap);
            dialog.Owner = MainWindow;
            dialog.Show();

            return Task.CompletedTask;
        }

        private Task PreviewImage(ThemeBitmapViewModel bitmap)
        {
            if (bitmap == null)
                return Task.CompletedTask;

            var vm = new PreviewImageDialogViewModel(bitmap.LoadDrawingBitmap());

            var dialog = new PreviewImageDialog();
            dialog.Owner = MainWindow;
            dialog.DataContext = vm;
            dialog.Show();

            return Task.CompletedTask;
        }

        private void AppendVisualStates(StringBuilder builder, ThemePartViewModel part)
        {
            var statesCount = part.States.Count;
            var durationProp = part.AllProperties.FirstOrDefault(x => x.PropertyId == TMT.TRANSITIONDURATIONS);
            var intList = durationProp?.Value as IntList;
            if (intList == null)
                return;

            if (intList.Count != statesCount * statesCount) {
                for (statesCount = 1; ; ++statesCount) {
                    if (statesCount * statesCount == intList.Count)
                        break;
                    if (statesCount * statesCount > intList.Count)
                        return;
                }
            }

            var groupName = part.Name;
            builder.AppendLine($"  <VisualStateGroup x:Name=\"{groupName}\">");

            builder.AppendLine("    <VisualStateGroup.Transitions>");

            string prefix = DetermineNamePrefix(part.States);
            for (int i = 0; i < statesCount; ++i) {
                for (int j = 0; j < statesCount; ++j) {
                    if (i == j)
                        continue;

                    var from = part.States[i];
                    var to = part.States[j];
                    var duration = TimeSpan.FromMilliseconds(intList[i * statesCount + j]);
                    builder.AppendFormat(
                        CultureInfo.InvariantCulture,
                        "      <VisualTransition From=\"{0}\" To=\"{1}\" GeneratedDuration=\"{2:g}\"/>",
                        GetStateName(from.Name, prefix),
                        GetStateName(to.Name, prefix),
                        duration);
                    builder.AppendLine();
                }
            }
            builder.AppendLine("    </VisualStateGroup.Transitions>");

            for (int i = 0; i < statesCount; ++i) {
                var state = part.States[i];
                builder.AppendLine($"    <VisualState x:Name=\"{GetStateName(state.Name, prefix)}\">");
                builder.AppendLine("    </VisualState>");
            }

            builder.AppendLine("  </VisualStateGroup>");
        }

        private string GetStateName(string name, string stripPrefix)
        {
            if (stripPrefix != null && name.StartsWith(stripPrefix)) {
                name = name.Substring(stripPrefix.Length);

                name = string.Concat(name.Split('_').Select(x => {
                    if (x.Length == 1)
                        return x.ToUpperInvariant();
                    return
                        x.Substring(0, 1).ToUpperInvariant() +
                        x.Substring(1).ToLowerInvariant();
                }));
            }

            return name;
        }

        private string DetermineNamePrefix(IReadOnlyList<ThemeStateViewModel> states)
        {
            if (states.Count == 0)
                return null;

            int idx = states[0].Name.IndexOf('_');
            if (idx == -1)
                return null;

            string possiblePrefix = states[0].Name.Substring(0, idx + 1);
            if (states.All(x => x.Name.StartsWith(possiblePrefix)))
                return possiblePrefix;

            return null;
        }

        private string FormatVisualStates(ThemeClassViewModel @class)
        {
            var builder = new StringBuilder();
            builder.AppendLine("<VisualStateManager.VisualStateGroups>");
            foreach (var part in @class.Parts) {
                AppendVisualStates(builder, part);
            }
            builder.AppendLine("</VisualStateManager.VisualStateGroups>");
            return builder.ToString();
        }

        private string FormatVisualStates(ThemePartViewModel part)
        {
            var builder = new StringBuilder();
            builder.AppendLine("<VisualStateManager.VisualStateGroups>");
            AppendVisualStates(builder, part);
            builder.AppendLine("</VisualStateManager.VisualStateGroups>");
            return builder.ToString();
        }

        private Task CopyVisualStates(object parameter)
        {
            var @class = parameter as ThemeClassViewModel;
            var part = parameter as ThemePartViewModel;
            if (@class != null)
                Clipboard.SetText(FormatVisualStates(@class));
            else if (part != null)
                Clipboard.SetText(FormatVisualStates(part));

            return Task.CompletedTask;
        }

        public async void TryLoadTheme(string styleFilePath)
        {
            try {
                var newThemeFile = await Task.Run(() => LoadTheme(styleFilePath));
                ThemeFile?.Dispose();
                ThemeFile = newThemeFile;
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Failed to open file");
            }
        }

        public async void TryLoadAndCompareThemes(string styleFilePath1, string styleFilePath2)
        {
            try {
                var theme1 = await Task.Run(() => LoadTheme(styleFilePath1));
                var theme2 = await Task.Run(() => LoadTheme(styleFilePath2));
                ThemeFile?.Dispose();
                CompareThemes(theme1, theme2);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message, "Failed to open file");
            }
        }

        private void CompareThemes(ThemeFileViewModel theme1, ThemeFileViewModel theme2)
        {
        }

        private void Open()
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "MSStyles (*.msstyles)|*.msstyles|All Files (*.*)|*.*";
            if (dialog.ShowDialog() != true)
                return;

            TryLoadTheme(dialog.FileName);
        }

        private void Compare()
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "MSStyles (*.msstyles)|*.msstyles|All Files (*.*)|*.*";

            dialog.Title = "Select First Theme";
            if (dialog.ShowDialog() != true)
                return;

            var firstTheme = dialog.FileName;

            dialog.Title = "Select Second Theme";
            if (dialog.ShowDialog() != true)
                return;

            var secondTheme = dialog.FileName;

            TryLoadAndCompareThemes(firstTheme, secondTheme);
        }

        private ThemeFileViewModel LoadTheme(string styleFilePath)
        {
            return new ThemeFileViewModel(
                ThemeFileLoader.LoadTheme(styleFilePath));
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }

        public ICommand OpenCommand { get; }
        public ICommand CompareCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand CopyVisualStatesCommand { get; }
        public ICommand SaveImageCommand { get; }
        public ICommand SaveUnpremultipliedImageCommand { get; }
        public ICommand TraceImageCommand { get; }
        public ICommand PreviewImageCommand { get; }

        public ThemeFileViewModel ThemeFile
        {
            get => themeFile;
            set => SetProperty(ref themeFile, value);
        }

        public ObservableCollection<ThemeRawProperty> AllProperties
        {
            get => allProperties;
            set => SetProperty(ref allProperties, value);
        }

        public List<TransitionDuration> TransitionDurations
        {
            get => transitionDurations;
            set => SetProperty(ref transitionDurations, value);
        }

        public object SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (SetProperty(ref selectedItem, value)) {
                    Content = value;

                    var @class = value as ThemeClassViewModel;
                    var part = value as ThemePartViewModel;
                    var state = value as ThemeStateViewModel;

                    AllProperties.Clear();

                    if (@class != null)
                        DumpThemePart(@class.ClassName);
                    else if (part != null)
                        DumpThemePart(part.Parent.ClassName, part);
                    else if (state != null)
                        DumpThemePart(state.Parent.Parent.ClassName, state.Parent, state);
                }
            }
        }

        public object Content
        {
            get => content;
            set => SetProperty(ref content, value);
        }

        public class TransitionDuration
        {
            public TransitionDuration(string stateFrom, string stateTo, uint? duration)
            {
                StateFrom = stateFrom;
                StateTo = stateTo;
                Duration = duration;
            }

            public string StateFrom { get; set; }
            public string StateTo { get; set; }
            public uint? Duration { get; set; }
        }

        private void DumpThemePart(string className, ThemePartViewModel part = null, ThemeStateViewModel state = null)
        {
            var interopHelper = new WindowInteropHelper(Application.Current.MainWindow);
            var hwnd = interopHelper.EnsureHandle();
            if (hwnd == IntPtr.Zero)
                return;

            Task.Run(
                () => {
                    try {
                        DumpThemePart(hwnd, className, part, state);
                    } catch (Exception ex) {
                        Console.WriteLine(ex);
                    }
                });
        }

        private void DumpThemePart(IntPtr hwnd, string className, ThemePartViewModel part, ThemeStateViewModel state)
        {
            hwnd = IntPtr.Zero;
            using (IThemeData nativeTheme = ThemeData.Open(hwnd, className))
            using (IThemeData theme = UxThemeExData.Open(
                    themeFile.ThemeFile.NativeThemeFile, hwnd, className)) {
                if (!theme.IsValid)
                    return;

                int partId = part?.Id ?? 0;
                int stateId = state?.Id ?? 0;
                AllProperties.Clear();
                LoadTransitions(theme, partId, part?.States);

                foreach (var entry in EnumThemeProperties()) {
                    int propId = entry.Item1;
                    foreach (var p in EnumPropertyValues(nativeTheme, partId, stateId, propId))
                        AllProperties.Add(p);
                    foreach (var p in EnumPropertyValues(theme, partId, stateId, propId))
                        AllProperties.Add(p);
                }
            }
        }

        private IEnumerable<ThemeRawProperty> EnumPropertyValues(
            IThemeData theme, int partId, int stateId, int propId)
        {
            var name = ((TMT)propId).ToString();
            var origin = theme.GetThemePropertyOrigin(partId, stateId, propId);
            var type = ThemeInfo.GetPropertyType((TMT)propId);

            object value = null;
            switch (type) {
                case ThemePropertyType.Enum:
                    value = theme.GetThemeEnumValue(partId, stateId, propId);
                    break;
                case ThemePropertyType.String:
                    value = theme.GetThemeString(partId, stateId, propId);
                    break;
                case ThemePropertyType.Int:
                    value = theme.GetThemeInt(partId, stateId, propId);
                    break;
                case ThemePropertyType.Bool:
                    value = theme.GetThemeBool(partId, stateId, propId);
                    break;
                case ThemePropertyType.Color:
                    value = theme.GetThemeColor(partId, stateId, propId);
                    break;
                case ThemePropertyType.Margins:
                    value = theme.GetThemeMargins(partId, stateId, propId);
                    break;
                case ThemePropertyType.Filename:
                    value = theme.GetThemeFilename(partId, stateId, propId);
                    break;
                case ThemePropertyType.Size:
                    var partSize1 = theme.GetThemePartSize(partId, stateId, ThemeSize.Min);
                    var partSize2 = theme.GetThemePartSize(partId, stateId, ThemeSize.True);
                    var partSize3 = theme.GetThemePartSize(partId, stateId, ThemeSize.Draw);
                    if (partSize1 != null)
                        yield return new ThemeRawProperty(propId, $"{name} (Min)", type, origin, partSize1);
                    if (partSize2 != null)
                        yield return new ThemeRawProperty(propId, $"{name} (True)", type, origin, partSize2);
                    if (partSize3 != null)
                        yield return new ThemeRawProperty(propId, $"{name} (Draw)", type, origin, partSize3);
                    break;
                case ThemePropertyType.Position:
                    value = theme.GetThemePosition(partId, stateId, propId);
                    break;
                case ThemePropertyType.Rect:
                    value = theme.GetThemeRect(partId, stateId, propId);
                    break;
                case ThemePropertyType.Font:
                    value = theme.GetThemeFont(partId, stateId, propId);
                    break;
                case ThemePropertyType.IntList:
                    value = theme.GetThemeIntList(partId, stateId, propId);
                    break;
                case ThemePropertyType.HBitmap:
                    var hbmp = theme.GetThemeBitmap(partId, stateId, propId);
                    if (hbmp != null) {
                        var source = Imaging.CreateBitmapSourceFromHBitmap(
                            hbmp.Value, IntPtr.Zero, Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                        source.Freeze();
                        value = source;
                    }
                    break;
                case ThemePropertyType.DiskStream:
                    value = theme.GetThemeStream(partId, stateId, propId, SafeModuleHandle.Zero);
                    break;
                case ThemePropertyType.Stream:
                    value = theme.GetThemeStream(partId, stateId, propId, SafeModuleHandle.Zero);
                    break;
                case ThemePropertyType.BitmapRef:
                    value = theme.GetThemeBitmap(partId, stateId, propId);
                    break;
                case ThemePropertyType.None:
                    break;
                default:
                    value =
                        theme.GetThemeEnumValue(partId, stateId, propId) ??
                        theme.GetThemeString(partId, stateId, propId) ??
                        theme.GetThemeInt(partId, stateId, propId) ??
                        theme.GetThemeBool(partId, stateId, propId) ??
                        theme.GetThemeColor(partId, stateId, propId) ??
                        theme.GetThemeMargins(partId, stateId, propId) ??
                        theme.GetThemeFilename(partId, stateId, propId) ??
                        theme.GetThemePosition(partId, stateId, propId) ??
                        theme.GetThemeRect(partId, stateId, propId) ??
                        theme.GetThemeFont(partId, stateId, propId) ??
                        theme.GetThemeIntList(partId, stateId, propId) ??
                        theme.GetThemeBitmapAsImageSource(partId, stateId, propId) ??
                        theme.GetThemeStream(partId, stateId, propId, SafeModuleHandle.Zero) ??
                        (object)$"<Unhandled Property Type '{type}'>";
                    break;
            }

            if (value != null)
                yield return new ThemeRawProperty(
                    propId, name, type, origin, value);
        }

        private IEnumerable<Tuple<int, string>> EnumThemeProperties()
        {
            int maxPropId = ThemeInfo.Entries<TMT>().Max(x => x.Item1);

            for (int i = 0; i < maxPropId; ++i) {
                yield return Tuple.Create(i, ((TMT)i).ToString());
            }
        }

        public void LoadTransitions(IThemeData theme, int partId, IReadOnlyList<ThemeStateViewModel> states)
        {
            var durations = new List<TransitionDuration>();

            if (states != null) {
                var stateIds = Enumerable.Range(1, states.Count);
                foreach (var stateFrom in stateIds) {
                    foreach (var stateTo in stateIds) {
                        uint duration;
                        HResult hr = theme.GetThemeTransitionDuration(partId, stateFrom, stateTo, (int)TMT.TRANSITIONDURATIONS, out duration);
                        if (hr.Succeeded())
                            durations.Add(new TransitionDuration(
                                states[stateFrom - 1].DisplayName,
                                states[stateTo - 1].DisplayName,
                                duration));
                    }
                }
            }

            TransitionDurations = durations;
        }

        public ContextMenu GetTreeContextMenu()
        {
            if (SelectedItem == null)
                return null;


            var @class = SelectedItem as ThemeClassViewModel;
            if (@class != null) {
                var menu = new ContextMenu();
                menu.Items.Add(new MenuItem {
                    Header = "Copy Visual States to clipboard",
                    Command = CopyVisualStatesCommand,
                    CommandParameter = @class
                });
                return menu;
            }

            var part = SelectedItem as ThemePartViewModel;
            if (part != null) {
                var menu = new ContextMenu();
                menu.Items.Add(new MenuItem {
                    Header = "Copy Visual States to clipboard",
                    Command = CopyVisualStatesCommand,
                    CommandParameter = part
                });
                return menu;
            }

            return null;
        }
    }

    internal class BitmapValue
    {
        public BitmapValue(BitmapSource source)
        {
            Source = source;
        }

        public BitmapSource Source { get; }
    }

    public class ThemeRawProperty
    {
        public ThemeRawProperty(
            int propId, string name, ThemePropertyType type, PropertyOrigin origin, object value)
        {
            PropId = propId;
            Name = name;
            Type = type;
            Origin = origin;
            Value = value;
        }

        public int PropId { get; }
        public string Name { get; }
        public ThemePropertyType Type { get; }
        public PropertyOrigin Origin { get; }
        public object Value { get; }
    }

    public static class ThemeDataExtensions
    {
        public static ImageSource GetThemeBitmapAsImageSource(
            this IThemeData themeData, int partId, int stateId, int propertyId)
        {
            var hbmp = themeData.GetThemeBitmap(partId, stateId, propertyId);
            if (hbmp == null)
                return null;

            var source = Imaging.CreateBitmapSourceFromHBitmap(
                hbmp.Value, IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            return source;
        }
    }
}