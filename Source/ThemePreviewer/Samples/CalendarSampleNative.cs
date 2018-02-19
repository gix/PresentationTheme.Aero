namespace ThemePreviewer.Samples
{
    using System.Collections.Generic;
    using System.Windows.Forms;

    public partial class CalendarSampleNative : UserControl, IOptionControl
    {
        private readonly OptionList options = new OptionList();

        public CalendarSampleNative()
        {
            InitializeComponent();

            options.AddOption("Enabled", calendar1, x => x.Enabled);
            options.AddOption("ShowToday", calendar1, x => x.ShowToday);
            options.AddOption("ShowWeekNumbers", calendar1, x => x.ShowWeekNumbers);
        }

        public IReadOnlyList<Option> Options => options;
    }
}
