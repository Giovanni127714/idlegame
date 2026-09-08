using System;
using System.Globalization;
using Idlegame.Helpers;
using Idlegame.Models;

namespace Idlegame.ViewModels
{
    /// <summary>
    /// Presentatie-wrapper rond een ShopItem. In tegenstelling tot Upgrade en
    /// Automation is dit herhaaldelijk aankoopbaar: elke aankoop verhoogt het
    /// aantal en dus de prijs van de volgende aankoop (schaalt exponentieel).
    /// </summary>
    public class ShopItemViewModel : ObservableObject
    {
        private string _costDisplay = string.Empty;
        private int _quantity;
        private bool _canPurchase;
        private string _statusText = string.Empty;

        public ShopItemViewModel(ShopItem model, Action<ShopItemViewModel> onBuyRequested)
        {
            Model = model;
            BuyCommand = new RelayCommand(_ => onBuyRequested(this), _ => CanPurchase);

            ProductionDisplay = string.Format(
                CultureInfo.InvariantCulture,
                "+{0:N2} / sec per stuk",
                model.IncomePerSecondPerUnit);

            Quantity = model.Quantity;
            RefreshCostDisplay();
        }

        public ShopItem Model { get; }

        public string Id => Model.Id;

        public string Name => Model.Name;

        public string Description => Model.Description;

        public string ProductionDisplay { get; }

        public string CostDisplay
        {
            get => _costDisplay;
            private set => SetField(ref _costDisplay, value);
        }

        public int Quantity
        {
            get => _quantity;
            private set => SetField(ref _quantity, value);
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

        public void ApplyPurchase()
        {
            Model.Quantity++;
            Quantity = Model.Quantity;
            RefreshCostDisplay();
        }

        /// <summary>Zet het aantal direct (bv. bij het laden van een save), zonder kosten te verrekenen.</summary>
        public void SetQuantity(int quantity)
        {
            Model.Quantity = quantity;
            Quantity = quantity;
            RefreshCostDisplay();
        }

        private void RefreshCostDisplay()
        {
            CostDisplay = Model.CurrentCost.ToString("N0", CultureInfo.InvariantCulture);
        }
    }
}
