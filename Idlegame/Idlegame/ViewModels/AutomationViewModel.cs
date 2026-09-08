using System;
using System.Globalization;
using System.Windows.Threading;
using Idlegame.Helpers;
using Idlegame.Models;

namespace Idlegame.ViewModels
{
    /// <summary>
    /// Presentatie-wrapper rond een Automation-model. Zodra ontgrendeld start
    /// deze zijn eigen, onafhankelijke DispatcherTimer die op het ingestelde
    /// interval blijft produceren, los van de globale gameloop-tick of van
    /// andere automatiseringen.
    /// </summary>
    public class AutomationViewModel : ObservableObject
    {
        private readonly DispatcherTimer _timer;
        private readonly Action<AutomationViewModel> _onProduce;

        private bool _isUnlocked;
        private bool _canPurchase;
        private string _statusText = string.Empty;

        public AutomationViewModel(Automation model, Action<AutomationViewModel> onBuyRequested, Action<AutomationViewModel> onProduce)
        {
            Model = model;
            _onProduce = onProduce;

            BuyCommand = new RelayCommand(_ => onBuyRequested(this), _ => CanPurchase);

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(model.IntervalSeconds)
            };
            _timer.Tick += (_, _) => _onProduce(this);

            CostDisplay = model.Cost.ToString("N0", CultureInfo.InvariantCulture);
            ProductionDisplay = string.Format(
                CultureInfo.InvariantCulture,
                "+{0:N1} elke {1:N0}s",
                model.ProductionPerTick,
                model.IntervalSeconds);
        }

        public Automation Model { get; }

        public string Id => Model.Id;

        public string Name => Model.Name;

        public string Description => Model.Description;

        public string CostDisplay { get; }

        public string ProductionDisplay { get; }

        public bool IsUnlocked
        {
            get => _isUnlocked;
            private set => SetField(ref _isUnlocked, value);
        }

        public bool CanPurchase
        {
            get => _canPurchase;
            set
            {
                if (SetField(ref _canPurchase, value))
                {
                    BuyCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            set => SetField(ref _statusText, value);
        }

        public RelayCommand BuyCommand { get; }

        public void Unlock()
        {
            if (IsUnlocked)
            {
                return;
            }

            Model.IsUnlocked = true;
            IsUnlocked = true;
            _timer.Start();
        }
    }
}
