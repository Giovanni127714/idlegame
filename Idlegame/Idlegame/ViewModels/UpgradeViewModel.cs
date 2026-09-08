using System;
using System.Globalization;
using Idlegame.Helpers;
using Idlegame.Models;

namespace Idlegame.ViewModels
{
    /// <summary>
    /// Presentatie-wrapper rond een Upgrade-model: bevat de statische
    /// weergavetekst en de state (betaalbaar/gekocht) die verandert
    /// terwijl het spel loopt.
    /// </summary>
    public class UpgradeViewModel : ObservableObject
    {
        private bool _isPurchased;
        private bool _canPurchase;
        private string _statusText = string.Empty;

        public UpgradeViewModel(Upgrade model, Action<UpgradeViewModel> onBuyRequested)
        {
            Model = model;
            BuyCommand = new RelayCommand(_ => onBuyRequested(this), _ => CanPurchase);

            CostDisplay = model.Cost.ToString("N0", CultureInfo.InvariantCulture);
            EffectDisplay = model.EffectType switch
            {
                UpgradeEffectType.ClickValue => $"+{model.EffectAmount.ToString("N1", CultureInfo.InvariantCulture)} per klik",
                UpgradeEffectType.IncomePerSecond => $"+{model.EffectAmount.ToString("N1", CultureInfo.InvariantCulture)} / sec",
                _ => string.Empty
            };
        }

        public Upgrade Model { get; }

        public string Id => Model.Id;

        public string Name => Model.Name;

        public string Description => Model.Description;

        public string CostDisplay { get; }

        public string EffectDisplay { get; }

        public string? RequiresUpgradeId => Model.RequiresUpgradeId;

        public bool IsPurchased
        {
            get => _isPurchased;
            set => SetField(ref _isPurchased, value);
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
    }
}
