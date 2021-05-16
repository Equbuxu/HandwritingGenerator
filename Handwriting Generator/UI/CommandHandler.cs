using System;
using System.Windows.Input;

namespace Handwriting_Generator.UI
{
    class CommandHandler : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action<object> action;
        public CommandHandler(Action<object> action)
        {
            this.action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            action(parameter);
        }
    }
}
