using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using Idlegame.Helpers;
using Idlegame.Models;
using Idlegame.Services;

namespace Idlegame.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private const int MaxLogEntries = 50;
        private const double PrestigeThreshold = 1000.0;
        private static readonly TimeSpan AutosaveInterval = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan MinOfflineNoticeDuration = TimeSpan.FromSeconds(10);

        private readonly GameState _gameState = new();
        private readonly GameLoopService _gameLoop;
        private readonly SaveService _saveService = new();
        private readonly DispatcherTimer _autosaveTimer;

        private string _currencyDisplay = string.Empty;
        private string _incomePerSecondDisplay = string.Empty;
        private string _clickValueDisplay = string.Empty;
        private string _lastSavedDisplay = "Nog niet opgeslagen";
        private string _prestigePointsDisplay = string.Empty;
        private string _prestigeMultiplierDisplay = string.Empty;
        private string _prestigeAvailableDisplay = string.Empty;
        private bool _canPrestige;

        public MainViewModel()
        {
            _gameLoop = new GameLoopService();
            _gameLoop.Tick += OnGameTick;

            ClickCommand = new RelayCommand(_ => OnClick());
            SaveCommand = new RelayCommand(_ => SaveGame());
            PrestigeCommand = new RelayCommand(_ => TryPrestige(), _ => CanPrestige);

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

            LoadGame();

            UpdateDisplays();
            RefreshAffordability();
            _gameLoop.Start();
            _autosaveTimer.Start();
        }

        public RelayCommand ClickCommand { get; }

        public RelayCommand SaveCommand { get; }

        public RelayCommand PrestigeCommand { get; }

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

        public string PrestigePointsDisplay
        {
            get => _prestigePointsDisplay;
            private set => SetField(ref _prestigePointsDisplay, value);
        }

        public string PrestigeMultiplierDisplay
        {
            get => _prestigeMultiplierDisplay;
            private set => SetField(ref _prestigeMultiplierDisplay, value);
        }

        public string PrestigeAvailableDisplay
        {
            get => _prestigeAvailableDisplay;
            private set => SetField(ref _prestigeAvailableDisplay, value);
        }

        public bool CanPrestige
        {
            get => _canPrestige;
            private set
            {
                if (SetField(ref _canPrestige, value))
                {
                    PrestigeCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private void OnGameTick(double elapsedSeconds)
        {
            double amount = GetContinuousIncomePerSecond() * elapsedSeconds;
            _gameState.Currency += amount;
            _gameState.TotalEarned += amount;
            UpdateDisplays();
            RefreshAffordability();
        }

        private void OnClick()
        {
            double amount = _gameState.ClickValue * _gameState.PrestigeMultiplier;
            _gameState.Currency += amount;
            _gameState.TotalEarned += amount;
            UpdateDisplays();
            RefreshAffordability();
        }

        private void OnAutomationProduce(AutomationViewModel automation)
        {
            double amount = automation.Model.ProductionPerTick * _gameState.PrestigeMultiplier;
            _gameState.Currency += amount;
            _gameState.TotalEarned += amount;
            AddLogEntry($"{automation.Name} produceerde +{amount.ToString("N1", CultureInfo.InvariantCulture)}");

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

        private void LoadGame()
        {
            try
            {
                if (!_saveService.TryLoad(out var data) || data is null)
                {
                    AddLogEntry("Geen eerdere save gevonden, nieuw spel gestart");
                    return;
                }

                ApplySaveData(data);
                AddLogEntry($"Voortgang geladen (opgeslagen op {data.SavedAtUtc.ToLocalTime():dd-MM-yyyy HH:mm:ss})");
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
            {
                // Save-bestand is corrupt of onleesbaar: GameState en collecties staan
                // nog op hun veilige default-waarden (ApplySaveData is niet uitgevoerd),
                // dus we gaan gewoon verder met een nieuw spel na de foutmelding.
                AddLogEntry($"Save-bestand kon niet geladen worden ({ex.Message}). Gestart met een veilige standaardstatus.");
                MessageBox.Show(
                    "Het savebestand is beschadigd en kon niet geladen worden. Er is gestart met een nieuw spel.",
                    "Laden mislukt",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void ApplySaveData(SaveData data)
        {
            _gameState.Currency = data.Currency;
            _gameState.TotalEarned = data.TotalEarned;
            _gameState.IncomePerSecond = data.IncomePerSecond;
            _gameState.ClickValue = data.ClickValue;
            _gameState.PrestigePoints = data.PrestigePoints;

            foreach (var upgrade in Upgrades)
            {
                if (data.PurchasedUpgradeIds.Contains(upgrade.Id))
                {
                    upgrade.Model.IsPurchased = true;
                    upgrade.IsPurchased = true;
                }
            }

            foreach (var automation in Automations)
            {
                if (data.UnlockedAutomationIds.Contains(automation.Id))
                {
                    automation.Unlock();
                }
            }

            foreach (var shopItem in ShopItems)
            {
                if (data.ShopItemQuantities.TryGetValue(shopItem.Id, out int quantity))
                {
                    shopItem.SetQuantity(quantity);
                }
            }

            ApplyOfflineProgress(data.SavedAtUtc);
        }

        /// <summary>
        /// Berekent hoe lang de speler weg was sinds de laatste save en kent
        /// daarvoor valuta toe tegen het inkomen/sec-tarief dat gold bij het
        /// opslaan (inclusief upgrades, winkel-items en het gemiddelde van
        /// actieve automatiseringen). Bij een korte afwezigheid (bv. een
        /// snelle herstart tijdens het testen) blijft het stil in de HUD en
        /// alleen zichtbaar in het log; pas vanaf MinOfflineNoticeDuration
        /// verschijnt ook een welkomstmelding.
        /// </summary>
        private void ApplyOfflineProgress(DateTime savedAtUtc)
        {
            TimeSpan elapsed = DateTime.UtcNow - savedAtUtc;
            if (elapsed <= TimeSpan.Zero)
            {
                return;
            }

            double offlineIncomePerSecond = GetTotalIncomePerSecond();
            double offlineEarnings = offlineIncomePerSecond * elapsed.TotalSeconds;
            _gameState.Currency += offlineEarnings;
            _gameState.TotalEarned += offlineEarnings;

            string durationText = FormatDuration(elapsed);
            AddLogEntry($"Offline voortgang: {durationText} weg, +{offlineEarnings.ToString("N1", CultureInfo.InvariantCulture)} valuta verdiend");

            if (elapsed >= MinOfflineNoticeDuration && offlineEarnings > 0)
            {
                MessageBox.Show(
                    $"Welkom terug! Je was {durationText} weg en hebt in die tijd +{offlineEarnings.ToString("N1", CultureInfo.InvariantCulture)} valuta verdiend.",
                    "Offline voortgang",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalDays >= 1)
            {
                return $"{(int)duration.TotalDays} dag(en) en {duration.Hours} uur";
            }

            if (duration.TotalHours >= 1)
            {
                return $"{(int)duration.TotalHours} uur en {duration.Minutes} minuten";
            }

            if (duration.TotalMinutes >= 1)
            {
                return $"{(int)duration.TotalMinutes} minuten en {duration.Seconds} seconden";
            }

            return $"{(int)duration.TotalSeconds} seconden";
        }

        private void SaveGame()
        {
            var data = new SaveData
            {
                Currency = _gameState.Currency,
                TotalEarned = _gameState.TotalEarned,
                IncomePerSecond = _gameState.IncomePerSecond,
                ClickValue = _gameState.ClickValue,
                PrestigePoints = _gameState.PrestigePoints,
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

        /// <summary>
        /// Aantal prestigepunten dat de speler nu zou krijgen bij het
        /// prestigen, gebaseerd op de ooit-verdiende valuta (niet de
        /// huidige, uitgeefbare valuta - anders zou uitgeven vlak voor
        /// prestige lonen). Vierkantswortel-schaal zodat elk volgend punt
        /// meer moeite kost, wat herhaald prestigen zinvol maakt.
        /// </summary>
        private int GetAvailablePrestigePoints()
        {
            if (_gameState.TotalEarned < PrestigeThreshold)
            {
                return 0;
            }

            return (int)Math.Floor(Math.Sqrt(_gameState.TotalEarned / PrestigeThreshold));
        }

        private void TryPrestige()
        {
            int availablePoints = GetAvailablePrestigePoints();
            if (availablePoints < 1)
            {
                return;
            }

            int newTotal = _gameState.PrestigePoints + availablePoints;
            var confirm = MessageBox.Show(
                Application.Current.MainWindow,
                $"Je krijgt {availablePoints} ster(ren) en start opnieuw vanaf de basis: valuta, upgrades, " +
                "automatiseringen en de winkel worden gereset. Je totaal aantal sterren wordt " +
                $"{newTotal}, wat een permanente inkomensbonus van +{newTotal * 10}% geeft.\n\nDoorgaan?",
                "Prestige",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }

            _gameState.PrestigePoints = newTotal;
            _gameState.Currency = 0;
            _gameState.TotalEarned = 0;
            _gameState.IncomePerSecond = GameState.BaseIncomePerSecond;
            _gameState.ClickValue = GameState.BaseClickValue;

            foreach (var upgrade in Upgrades)
            {
                upgrade.Model.IsPurchased = false;
                upgrade.IsPurchased = false;
            }

            foreach (var automation in Automations)
            {
                automation.ResetToLocked();
            }

            foreach (var shopItem in ShopItems)
            {
                shopItem.SetQuantity(0);
            }

            AddLogEntry($"Prestige uitgevoerd: +{availablePoints} ster(ren) (totaal: {newTotal}, bonus: +{newTotal * 10}%)");

            UpdateDisplays();
            RefreshAffordability();
        }

        private void RefreshAffordability()
        {
            RefreshUpgradeStates();
            RefreshAutomationStates();
            RefreshShopStates();
            RefreshPrestigeState();
        }

        private void RefreshPrestigeState()
        {
            int availablePoints = GetAvailablePrestigePoints();
            CanPrestige = availablePoints >= 1;
            PrestigeAvailableDisplay = availablePoints.ToString(CultureInfo.InvariantCulture);
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
        /// (basisinkomen + upgrades + bezeten winkel-items) * prestige-
        /// vermenigvuldiger. Automatiseringen tellen hier niet in mee, want
        /// die produceren discreet op hun eigen timer (zie OnAutomationProduce).
        /// </summary>
        private double GetContinuousIncomePerSecond()
        {
            double shopRate = ShopItems.Sum(s => s.Model.Quantity * s.Model.IncomePerSecondPerUnit);
            return (_gameState.IncomePerSecond + shopRate) * _gameState.PrestigeMultiplier;
        }

        /// <summary>
        /// Doorlopend inkomen/sec plus het gemiddelde van actieve
        /// automatiseringen (productie/interval), beide al vermenigvuldigd
        /// met de prestige-bonus - het volledige tarief zoals getoond in de
        /// HUD, en de basis voor de offline-berekening.
        /// </summary>
        private double GetTotalIncomePerSecond()
        {
            double automationRate = Automations
                .Where(a => a.IsUnlocked)
                .Sum(a => a.Model.ProductionPerTick / a.Model.IntervalSeconds) * _gameState.PrestigeMultiplier;
            return GetContinuousIncomePerSecond() + automationRate;
        }

        private void UpdateDisplays()
        {
            CurrencyDisplay = _gameState.Currency.ToString("N1", CultureInfo.InvariantCulture);
            IncomePerSecondDisplay = GetTotalIncomePerSecond().ToString("N1", CultureInfo.InvariantCulture);
            ClickValueDisplay = (_gameState.ClickValue * _gameState.PrestigeMultiplier).ToString("N1", CultureInfo.InvariantCulture);
            PrestigePointsDisplay = _gameState.PrestigePoints.ToString(CultureInfo.InvariantCulture);
            PrestigeMultiplierDisplay = "x" + _gameState.PrestigeMultiplier.ToString("N2", CultureInfo.InvariantCulture);
        }
    }
}
