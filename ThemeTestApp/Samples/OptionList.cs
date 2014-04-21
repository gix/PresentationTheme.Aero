namespace ThemeTestApp.Samples
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;

    internal class OptionList : Collection<Option>
    {
        public EventHandler<StateEventArgs> StateChanged;

        public Option AddOption<TControl>(
            TControl control,
            string name,
            Expression<Func<TControl, bool>> propertyExpression,
            bool negated = false)
        {
            var option = Option.Create(control, name, propertyExpression, negated);
            option.StateChanged += OnStateChanged;
            Add(option);

            return option;
        }

        public Option AddOption<TControl, T>(
            TControl control,
            string name,
            Expression<Func<TControl, T>> propertyExpression,
            T trueValue,
            T falseValue)
        {
            var option = Option.Create(control, name, propertyExpression, trueValue, falseValue);
            option.StateChanged += OnStateChanged;
            Add(option);

            return option;
        }

        private void OnStateChanged(object sender, StateEventArgs e)
        {
            var handler = StateChanged;
            if (handler != null)
                handler(sender, e);
            RereadOptions();
        }

        private void RereadOptions()
        {
            foreach (var option in this)
                option.Refresh();
        }
    }
}