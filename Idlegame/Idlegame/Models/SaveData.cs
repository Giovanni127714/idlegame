using System;
using System.Collections.Generic;

namespace Idlegame.Models
{
    /// <summary>
    /// Alles wat nodig is om een sessie exact te reconstrueren: valuta en
    /// basisstats, welke upgrades gekocht zijn, welke automatiseringen (met
    /// hun eigen timer) ontgrendeld zijn, het aantal bezeten winkel-items,
    /// en het moment van opslaan.
    /// </summary>
    public class SaveData
    {
        public double Currency { get; set; }

        public double TotalEarned { get; set; }

        public double IncomePerSecond { get; set; }

        public double ClickValue { get; set; }

        public int PrestigePoints { get; set; }

        public List<string> PurchasedUpgradeIds { get; set; } = new();

        public List<string> UnlockedAutomationIds { get; set; } = new();

        public Dictionary<string, int> ShopItemQuantities { get; set; } = new();

        public DateTime SavedAtUtc { get; set; }
    }
}
