using System;
using System.Windows.Input;

namespace Idlegame.Helpers
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Eigen event i.p.v. CommandManager.RequerySuggested: onze CanExecute-state
        // verandert door een achtergrond-timer (game loop), niet door UI-input, dus
        // moet expliciet ongeldig gemaakt worden via RaiseCanExecuteChanged().
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _execute(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
