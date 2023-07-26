using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JsonReader.Commands
{
    public class RelayCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        private Predicate<object?> _canExecute;
        private Action<object?> _execute;

        public RelayCommand(Action<object?> execute, Predicate<object?> canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(execute);
            _canExecute = canExecute;
            _execute = execute;
        }

        public bool CanExecute(object? parameter) => _canExecute is null || _canExecute.Invoke(parameter);

        public void Execute(object? parameter) => _execute.Invoke(parameter);
    }
}
