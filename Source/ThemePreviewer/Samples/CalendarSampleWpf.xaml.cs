namespace ThemePreviewer.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;

    public partial class CalendarSampleWpf : IOptionControl
    {
        private readonly OptionList options = new OptionList();

        public CalendarSampleWpf()
        {
            InitializeComponent();

            options.AddOption("Enabled", calendar1, x => x.IsEnabled);
            options.AddEnumOption("DisplayMode", calendar1, x => x.DisplayMode);
            options.AddEnumOption("SelectionMode", calendar1, x => x.SelectionMode);
            options.AddOption("IsTodayHighlighted", calendar1, x => x.IsTodayHighlighted);

            var today = DateTime.Now.Date;
            calendar1.BlackoutDates.Add(
                new CalendarDateRange(
                    new DateTime(today.Year, today.Month, today.Day == 1 ? 2 : 1),
                    new DateTime(today.Year, today.Month, 3)));
            calendar1.SelectedDate = today;
        }

        public IReadOnlyList<Option> Options => options;
    }
}
