namespace ThemePreviewer
{
    using System;
    using System.Windows.Input;

    public class DelegateCommand : ICommand
    {
        private readonly Action action;

        public DelegateCommand(Action action)
        {
            this.action = action;
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            action();
        }
    }

    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> action;

        public DelegateCommand(Action<T> action)
        {
            this.action = action;
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter)
        {
            return parameter == null || parameter is T;
        }

        public void Execute(object parameter)
        {
            if (parameter is T || (default(T) == null && parameter == null))
                action((T)parameter);
        }
    }
}
