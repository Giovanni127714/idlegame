using System.Globalization;
using Idlegame.Helpers;
using Idlegame.Models;
using Idlegame.Services;

namespace Idlegame.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly GameState _gameState = new();
        private readonly GameLoopService _gameLoop;

        private string _currencyDisplay = string.Empty;
        private string _incomePerSecondDisplay = string.Empty;

        public MainViewModel()
        {
            _gameLoop = new GameLoopService();
            _gameLoop.Tick += OnGameTick;

            UpdateDisplays();
            _gameLoop.Start();
        }

        public string CurrencyDisplay
        {
            get => _currencyDisplay;
            private set => SetField(ref _currencyDisplay, value);
        }

        public string IncomePerSecondDisplay
        {
            get => _incomePerSecondDisplay;
            private set => SetField(ref _incomePerSecondDisplay, value);
        }

        private void OnGameTick(double elapsedSeconds)
        {
            _gameState.Currency += _gameState.IncomePerSecond * elapsedSeconds;
            UpdateDisplays();
        }

        private void UpdateDisplays()
        {
            CurrencyDisplay = _gameState.Currency.ToString("N1", CultureInfo.InvariantCulture);
            IncomePerSecondDisplay = _gameState.IncomePerSecond.ToString("N1", CultureInfo.InvariantCulture);
        }
    }
}
