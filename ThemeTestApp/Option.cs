namespace ThemeTestApp
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;

    public class StateEventArgs : EventArgs
    {
        public StateEventArgs(bool enabled)
        {
            Enabled = enabled;
        }

        public bool Enabled { get; private set; }
    }

    public abstract class Option : INotifyPropertyChanged
    {
        public abstract bool Enabled { get; set; }
        public abstract string Label { get; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Refresh()
        {
            Enabled = Enabled;
            RaisePropertyChanged("Enabled");
        }

        public static Option<T> Create<T, TValue>(T control, string name, Expression<Func<T, TValue>> propertyExpression, TValue trueValue, TValue falseValue)
        {
            name = name.Replace("_", "__");

            var property = control.GetType().GetProperty(((MemberExpression)propertyExpression.Body).Member.Name);
            if (property == null)
                throw new ArgumentException("propertyName");

            var option = new Option<T>(
                control,
                name,
                c => ((TValue)property.GetValue(control, null)).Equals(trueValue),
                (c, v) => property.SetValue(control, v ? trueValue : falseValue, null));

            return option;
        }


        public static Option<T> Create<T>(T control, string name, Expression<Func<T, bool>> propertyExpression, bool negated = false)
        {
            name = name.Replace("_", "__");

            var property = control.GetType().GetProperty(((MemberExpression)propertyExpression.Body).Member.Name);
            if (property == null)
                throw new ArgumentException("propertyName");

            Option<T> option;
            if (negated)
                option = new Option<T>(
                    control,
                    name,
                    c => !(bool)property.GetValue(control, null),
                    (c, value) => property.SetValue(control, !value, null));
            else
                option = new Option<T>(
                    control,
                    name,
                    c => (bool)property.GetValue(control, null),
                    (c, value) => property.SetValue(control, value, null));

            return option;
        }
    }

    public class Option<T> : Option
    {
        private readonly T control;
        private readonly Func<T, bool> getter;
        private readonly Action<T, bool> setter;
        private readonly string label;

        public Option(T control, string label, Func<T, bool> getter, Action<T, bool> setter)
        {
            this.control = control;
            this.getter = getter;
            this.setter = setter;
            this.label = label;
        }

        public EventHandler<StateEventArgs> StateChanged;

        public override string Label
        {
            get { return label; }
        }

        public override bool Enabled
        {
            get { return getter(control); }
            set
            {
                if (Enabled != value) {
                    RaisePropertyChanged("Enabled");
                    setter(control, value);
                    var handler = StateChanged;
                    if (handler != null)
                        handler(this, new StateEventArgs(value));
                }
            }
        }
    }
}