using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Idlegame.Helpers;
using Idlegame.Models;
using Idlegame.Services;

namespace Idlegame.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private const int MaxLogEntries = 50;

        private readonly GameState _gameState = new();
        private readonly GameLoopService _gameLoop;

        private string _currencyDisplay = string.Empty;
        private string _incomePerSecondDisplay = string.Empty;
        private string _clickValueDisplay = string.Empty;

        public MainViewModel()
        {
            _gameLoop = new GameLoopService();
            _gameLoop.Tick += OnGameTick;

            ClickCommand = new RelayCommand(_ => OnClick());

            Upgrades = new ObservableCollection<UpgradeViewModel>(
                UpgradeCatalog.CreateDefault().Select(upgrade => new UpgradeViewModel(upgrade, TryPurchaseUpgrade)));

            Automations = new ObservableCollection<AutomationViewModel>(
                AutomationCatalog.CreateDefault().Select(automation =>
                    new AutomationViewModel(automation, TryPurchaseAutomation, OnAutomationProduce)));

            ActivityLog = new ObservableCollection<string>();

            UpdateDisplays();
            RefreshAffordability();
            _gameLoop.Start();
        }

        public RelayCommand ClickCommand { get; }

        public ObservableCollection<UpgradeViewModel> Upgrades { get; }

        public ObservableCollection<AutomationViewModel> Automations { get; }

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

        private void OnGameTick(double elapsedSeconds)
        {
            _gameState.Currency += _gameState.IncomePerSecond * elapsedSeconds;
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

        private void RefreshAffordability()
        {
            RefreshUpgradeStates();
            RefreshAutomationStates();
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

        private void AddLogEntry(string message)
        {
            ActivityLog.Insert(0, $"{DateTime.Now:HH:mm:ss} - {message}");

            while (ActivityLog.Count > MaxLogEntries)
            {
                ActivityLog.RemoveAt(ActivityLog.Count - 1);
            }
        }

        private void UpdateDisplays()
        {
            double automationRate = Automations
                .Where(a => a.IsUnlocked)
                .Sum(a => a.Model.ProductionPerTick / a.Model.IntervalSeconds);
            double totalIncomePerSecond = _gameState.IncomePerSecond + automationRate;

            CurrencyDisplay = _gameState.Currency.ToString("N1", CultureInfo.InvariantCulture);
            IncomePerSecondDisplay = totalIncomePerSecond.ToString("N1", CultureInfo.InvariantCulture);
            ClickValueDisplay = _gameState.ClickValue.ToString("N1", CultureInfo.InvariantCulture);
        }
    }
}
