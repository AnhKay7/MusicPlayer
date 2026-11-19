using System;
using System.Windows.Input;

namespace MusicPlayer.ViewModels
{
    public class RelayCommand : ICommand
    {
        private Action execute;
        private Func<bool>? canExecute = null;

        public RelayCommand(Action action, Func<bool>? func = null)
        {
            execute = action;
            canExecute = func;
        }

        public bool CanExecute(object? parameter)
        {
            return canExecute?.Invoke() ?? true;
        }

        public void Execute(object? parameter)
        {
            execute.Invoke();
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
