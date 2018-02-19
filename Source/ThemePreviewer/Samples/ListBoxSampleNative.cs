namespace ThemePreviewer.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Windows.Forms;

    public partial class ListBoxSampleNative : UserControl, IOptionControl
    {
        private readonly OptionList options;

        public IReadOnlyList<Option> Options => options;

        public ListBoxSampleNative()
        {
            InitializeComponent();

            sysListBox.Items.AddRange(CreateItems().ToArray<object>());

            options = new OptionList();
            CreateOptions();
        }

        private IEnumerable<string> CreateItems()
        {
            return ItemGenerator.Generate(90).Select(x => x[0]);
        }

        private void AddOption(string name, Expression<Func<ListBox, bool>> expression,
            bool negated = false)
        {
            options.AddOption(name, sysListBox, expression, negated);
        }

        private void AddOption<T>(
            string name, Expression<Func<ListBox, T>> expression, T trueValue, T falseValue)
        {
            options.AddOption(name, sysListBox, expression, trueValue, falseValue);
        }

        private void CreateOptions()
        {
            AddOption("Enabled", l => l.Enabled);

            //AddOption("LBS_NOTIFY", l => l.);
            AddOption("LBS_SORT", l => l.Sorted);
            //AddOption("LBS_NOREDRAW", l => l.);
            AddOption("LBS_MULTIPLESEL", l => l.SelectionMode, SelectionMode.MultiSimple, SelectionMode.None);
            //AddOption("LBS_OWNERDRAWFIXED", l => l.);
            //AddOption("LBS_OWNERDRAWVARIABLE", l => l.);
            //AddOption("LBS_HASSTRINGS", l => l.);
            AddOption("LBS_USETABSTOPS", l => l.UseTabStops);
            AddOption("LBS_NOINTEGRALHEIGHT", l => l.IntegralHeight, true);
            AddOption("LBS_MULTICOLUMN", l => l.MultiColumn);
            //AddOption("LBS_WANTKEYBOARDINPUT", l => l.);
            AddOption("LBS_EXTENDEDSEL", l => l.SelectionMode, SelectionMode.MultiExtended, SelectionMode.None);
            AddOption("LBS_DISABLENOSCROLL", l => l.ScrollAlwaysVisible);
            //AddOption("LBS_NODATA", l => l.);
            AddOption("LBS_NOSEL", l => l.SelectionMode, SelectionMode.MultiExtended, SelectionMode.None);
            //AddOption("LBS_COMBOBOX", l => l.);
            //AddOption("LBS_STANDARD", l => l.);
        }
    }
}
