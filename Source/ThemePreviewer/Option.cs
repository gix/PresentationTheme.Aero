namespace ThemePreviewer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;

    public class StateEventArgs : EventArgs
    {
        public StateEventArgs(bool enabled)
        {
            Enabled = enabled;
        }

        public bool Enabled { get; }
    }

    public abstract class Option : INotifyPropertyChanged
    {
        public abstract string Label { get; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void Refresh()
        {
        }

        public static BoolControlOption<T> Create<T, TValue>(
            T[] controls,
            string name,
            Expression<Func<T, TValue>> propertyExpression,
            TValue trueValue,
            TValue falseValue)
        {
            var property = typeof(T).GetProperty(((MemberExpression)propertyExpression.Body).Member.Name);
            if (property == null)
                throw new ArgumentException("propertyName");

            var option = new BoolControlOption<T>(
                controls,
                name,
                c => ((TValue)property.GetValue(c, null)).Equals(trueValue),
                (c, v) => property.SetValue(c, v ? trueValue : falseValue, null));

            return option;
        }


        public static BoolControlOption<T> Create<T>(
            T[] controls, string name, Expression<Func<T, bool>> propertyExpression,
            bool negated = false)
        {
            var property = propertyExpression.TryGetProperty();
            if (property == null)
                throw new ArgumentException("propertyName");

            BoolControlOption<T> option;
            if (negated)
                option = new BoolControlOption<T>(
                    controls,
                    name,
                    c => !(bool)property.GetValue(c, null),
                    (c, value) => property.SetValue(c, !value, null));
            else
                option = new BoolControlOption<T>(
                    controls,
                    name,
                    c => (bool)property.GetValue(c, null),
                    (c, value) => property.SetValue(c, value, null));

            return option;
        }
    }

    public sealed class SeparatorOption : Option
    {
        public override string Label { get; }
    }

    public abstract class BoolOption : Option
    {
        public abstract bool Enabled { get; set; }

        public override void Refresh()
        {
            Enabled = Enabled;
            RaisePropertyChanged(nameof(Enabled));
        }
    }

    public class BoolControlOption<T> : BoolOption
    {
        private readonly T[] controls;
        private readonly Func<T, bool> getter;
        private readonly Action<T, bool> setter;

        public BoolControlOption(
            T[] controls, string label, Func<T, bool> getter, Action<T, bool> setter)
        {
            this.controls = controls;
            this.getter = getter;
            this.setter = setter;
            Label = label;
        }

        public EventHandler<StateEventArgs> StateChanged;

        public override string Label { get; }

        public override bool Enabled
        {
            get { return getter(controls[0]); }
            set
            {
                if (Enabled != value) {
                    foreach (var control in controls)
                        setter(control, value);
                    RaisePropertyChanged(nameof(Enabled));
                    StateChanged?.Invoke(this, new StateEventArgs(value));
                }
            }
        }
    }

    public class GenericBoolOption : BoolOption
    {
        private readonly Func<bool> getter;
        private readonly Action<bool> setter;

        public GenericBoolOption(string label, Func<bool> getter, Action<bool> setter)
        {
            this.getter = getter;
            this.setter = setter;
            Label = label;
        }

        public event EventHandler<StateEventArgs> StateChanged;

        public override string Label { get; }

        public override bool Enabled
        {
            get { return getter(); }
            set
            {
                if (Enabled != value) {
                    setter(value);
                    RaisePropertyChanged(nameof(Enabled));
                    StateChanged?.Invoke(this, new StateEventArgs(value));
                }
            }
        }
    }

    public abstract class IntOption : Option
    {
        public abstract int Value { get; set; }

        public override void Refresh()
        {
            base.Refresh();
            Value = Value;
        }
    }

    public abstract class EnumOption : Option
    {
        public abstract object Value { get; set; }
        public abstract IReadOnlyList<object> Values { get; }

        public override void Refresh()
        {
            base.Refresh();
            Value = Value;
        }
    }

    public class EnumOption<T> : EnumOption
    {
        private readonly Func<T> getter;
        private readonly Action<T> setter;

        public override string Label { get; }
        public override IReadOnlyList<object> Values { get; }

        public EnumOption(string label, Func<T> getter, Action<T> setter)
        {
            this.getter = getter;
            this.setter = setter;
            Label = label;
            T[] values = (T[])Enum.GetValues(typeof(T));
            Values = values.Cast<object>().ToList();
        }

        public override object Value
        {
            get => TypedValue;
            set => TypedValue = (T)value;
        }

        public T TypedValue
        {
            get { return getter(); }
            set
            {
                if (!EqualityComparer<T>.Default.Equals(TypedValue, value)) {
                    setter(value);
                    RaisePropertyChanged(nameof(Value));
                }
            }
        }
    }

    public class GenericIntOption : IntOption
    {
        private readonly Func<int> getter;
        private readonly Action<int> setter;

        public GenericIntOption(string label, Func<int> getter, Action<int> setter)
        {
            this.getter = getter;
            this.setter = setter;
            Label = label;
        }

        public override string Label { get; }

        public override int Value
        {
            get { return getter(); }
            set
            {
                if (Value != value) {
                    setter(value);
                    RaisePropertyChanged(nameof(Value));
                }
            }
        }
    }

    public class IntControlOption<T> : IntOption
    {
        private readonly T[] controls;
        private readonly Func<T, int> getter;
        private readonly Action<T, int> setter;
        private int value;

        public IntControlOption(
            T[] controls, string label, Func<T, int> getter, Action<T, int> setter)
        {
            this.controls = controls;
            this.getter = getter;
            this.setter = setter;
            Label = label;
        }

        public override string Label { get; }

        public override int Value
        {
            get { return getter(controls[0]); }
            set
            {
                if (this.value != value) {
                    this.value = value;
                    foreach (var control in controls)
                        setter(control, value);
                    RaisePropertyChanged(nameof(Value));
                }
            }
        }
    }
}
