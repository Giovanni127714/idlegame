using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using Idlegame.Helpers;
using Idlegame.Models;
using Idlegame.Services;

namespace Idlegame.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private const int MaxLogEntries = 50;
        private static readonly TimeSpan AutosaveInterval = TimeSpan.FromSeconds(30);

        private readonly GameState _gameState = new();
        private readonly GameLoopService _gameLoop;
        private readonly SaveService _saveService = new();
        private readonly DispatcherTimer _autosaveTimer;

        private string _currencyDisplay = string.Empty;
        private string _incomePerSecondDisplay = string.Empty;
        private string _clickValueDisplay = string.Empty;
        private string _lastSavedDisplay = "Nog niet opgeslagen";

        public MainViewModel()
        {
            _gameLoop = new GameLoopService();
            _gameLoop.Tick += OnGameTick;

            ClickCommand = new RelayCommand(_ => OnClick());
            SaveCommand = new RelayCommand(_ => SaveGame());

            Upgrades = new ObservableCollection<UpgradeViewModel>(
                UpgradeCatalog.CreateDefault().Select(upgrade => new UpgradeViewModel(upgrade, TryPurchaseUpgrade)));

            Automations = new ObservableCollection<AutomationViewModel>(
                AutomationCatalog.CreateDefault().Select(automation =>
                    new AutomationViewModel(automation, TryPurchaseAutomation, OnAutomationProduce)));

            ShopItems = new ObservableCollection<ShopItemViewModel>(
                ShopCatalog.CreateDefault().Select(shopItem => new ShopItemViewModel(shopItem, TryPurchaseShopItem)));

            ActivityLog = new ObservableCollection<string>();

            _autosaveTimer = new DispatcherTimer { Interval = AutosaveInterval };
            _autosaveTimer.Tick += (_, _) => SaveGame();

            UpdateDisplays();
            RefreshAffordability();
            _gameLoop.Start();
            _autosaveTimer.Start();
        }

        public RelayCommand ClickCommand { get; }

        public RelayCommand SaveCommand { get; }

        public ObservableCollection<UpgradeViewModel> Upgrades { get; }

        public ObservableCollection<AutomationViewModel> Automations { get; }

        public ObservableCollection<ShopItemViewModel> ShopItems { get; }

        public ObservableCollection<string> ActivityLog { get; }

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

        public string ClickValueDisplay
        {
            get => _clickValueDisplay;
            private set => SetField(ref _clickValueDisplay, value);
        }

        public string LastSavedDisplay
        {
            get => _lastSavedDisplay;
            private set => SetField(ref _lastSavedDisplay, value);
        }

        private void OnGameTick(double elapsedSeconds)
        {
            _gameState.Currency += GetContinuousIncomePerSecond() * elapsedSeconds;
            UpdateDisplays();
            RefreshAffordability();
        }

        private void OnClick()
        {
            _gameState.Currency += _gameState.ClickValue;
            UpdateDisplays();
            RefreshAffordability();
        }

        private void OnAutomationProduce(AutomationViewModel automation)
        {
            _gameState.Currency += automation.Model.ProductionPerTick;
            AddLogEntry($"{automation.Name} produceerde +{automation.Model.ProductionPerTick.ToString("N1", CultureInfo.InvariantCulture)}");

            UpdateDisplays();
            RefreshAffordability();
        }

        private void TryPurchaseUpgrade(UpgradeViewModel upgrade)
        {
            if (!upgrade.CanPurchase)
            {
                return;
            }

            _gameState.Currency -= upgrade.Model.Cost;

            switch (upgrade.Model.EffectType)
            {
                case UpgradeEffectType.IncomePerSecond:
                    _gameState.IncomePerSecond += upgrade.Model.EffectAmount;
                    break;
                case UpgradeEffectType.ClickValue:
                    _gameState.ClickValue += upgrade.Model.EffectAmount;
                    break;
            }

            upgrade.Model.IsPurchased = true;
            upgrade.IsPurchased = true;

            AddLogEntry($"Upgrade '{upgrade.Name}' gekocht");

            UpdateDisplays();
            RefreshAffordability();
        }

        private void TryPurchaseAutomation(AutomationViewModel automation)
        {
            if (!automation.CanPurchase)
            {
                return;
            }

            _gameState.Currency -= automation.Model.Cost;
            automation.Unlock();

            AddLogEntry($"Automatisering '{automation.Name}' ontgrendeld");

            UpdateDisplays();
            RefreshAffordability();
        }

        private void TryPurchaseShopItem(ShopItemViewModel shopItem)
        {
            if (!shopItem.CanPurchase)
            {
                return;
            }

            _gameState.Currency -= shopItem.Model.CurrentCost;
            shopItem.ApplyPurchase();

            AddLogEntry($"'{shopItem.Name}' gekocht (aantal: {shopItem.Quantity})");

            UpdateDisplays();
            RefreshAffordability();
        }

        private void SaveGame()
        {
            var data = new SaveData
            {
                Currency = _gameState.Currency,
                IncomePerSecond = _gameState.IncomePerSecond,
                ClickValue = _gameState.ClickValue,
                PurchasedUpgradeIds = Upgrades.Where(u => u.IsPurchased).Select(u => u.Id).ToList(),
                UnlockedAutomationIds = Automations.Where(a => a.IsUnlocked).Select(a => a.Id).ToList(),
                ShopItemQuantities = ShopItems.ToDictionary(s => s.Id, s => s.Quantity),
                SavedAtUtc = DateTime.UtcNow
            };

            try
            {
                _saveService.Save(data);
                LastSavedDisplay = $"Laatst opgeslagen: {DateTime.Now:HH:mm:ss}";
                AddLogEntry("Spel opgeslagen");
            }
            catch (IOException ex)
            {
                AddLogEntry($"Opslaan mislukt: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                AddLogEntry($"Opslaan mislukt: {ex.Message}");
            }
        }

        private void RefreshAffordability()
        {
            RefreshUpgradeStates();
            RefreshAutomationStates();
            RefreshShopStates();
        }

        private void RefreshUpgradeStates()
        {
            foreach (var upgrade in Upgrades)
            {
                if (upgrade.IsPurchased)
                {
                    upgrade.CanPurchase = false;
                    upgrade.StatusText = "Gekocht";
                    continue;
                }

                bool requirementMet = upgrade.RequiresUpgradeId is null
                    || Upgrades.First(u => u.Id == upgrade.RequiresUpgradeId).IsPurchased;

                if (!requirementMet)
                {
                    var requiredUpgrade = Upgrades.First(u => u.Id == upgrade.RequiresUpgradeId);
                    upgrade.CanPurchase = false;
                    upgrade.StatusText = $"Vereist: {requiredUpgrade.Name}";
                    continue;
                }

                bool canAfford = _gameState.Currency >= upgrade.Model.Cost;
                upgrade.CanPurchase = canAfford;
                upgrade.StatusText = canAfford ? string.Empty : "Onvoldoende valuta";
            }
        }

        private void RefreshAutomationStates()
        {
            foreach (var automation in Automations)
            {
                if (automation.IsUnlocked)
                {
                    automation.CanPurchase = false;
                    automation.StatusText = "Actief";
                    continue;
                }

                bool canAfford = _gameState.Currency >= automation.Model.Cost;
                automation.CanPurchase = canAfford;
                automation.StatusText = canAfford ? string.Empty : "Onvoldoende valuta";
            }
        }

        private void RefreshShopStates()
        {
            foreach (var shopItem in ShopItems)
            {
                bool canAfford = _gameState.Currency >= shopItem.Model.CurrentCost;
                shopItem.CanPurchase = canAfford;
                shopItem.StatusText = canAfford ? string.Empty : "Onvoldoende valuta";
            }
        }

        private void AddLogEntry(string message)
        {
            ActivityLog.Insert(0, $"{DateTime.Now:HH:mm:ss} - {message}");

            while (ActivityLog.Count > MaxLogEntries)
            {
                ActivityLog.RemoveAt(ActivityLog.Count - 1);
            }
        }

        /// <summary>
        /// Inkomen/sec dat doorlopend (elke gameloop-tick) wordt bijgeteld:
        /// basisinkomen + upgrades + bezeten winkel-items. Automatiseringen
        /// tellen hier niet in mee, want die produceren discreet op hun eigen
        /// timer (zie OnAutomationProduce).
        /// </summary>
        private double GetContinuousIncomePerSecond()
        {
            double shopRate = ShopItems.Sum(s => s.Model.Quantity * s.Model.IncomePerSecondPerUnit);
            return _gameState.IncomePerSecond + shopRate;
        }

        private void UpdateDisplays()
        {
            double automationRate = Automations
                .Where(a => a.IsUnlocked)
                .Sum(a => a.Model.ProductionPerTick / a.Model.IntervalSeconds);
            double totalIncomePerSecond = GetContinuousIncomePerSecond() + automationRate;

            CurrencyDisplay = _gameState.Currency.ToString("N1", CultureInfo.InvariantCulture);
            IncomePerSecondDisplay = totalIncomePerSecond.ToString("N1", CultureInfo.InvariantCulture);
            ClickValueDisplay = _gameState.ClickValue.ToString("N1", CultureInfo.InvariantCulture);
        }
    }
}
