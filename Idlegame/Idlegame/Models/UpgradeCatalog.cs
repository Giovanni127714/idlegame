using System.Collections.Generic;

namespace Idlegame.Models
{
    /// <summary>
    /// Bron van de beschikbare upgrades. Voor nu hardcoded; kan later worden
    /// vervangen door een JSON-config zonder dat de consumers hoeven te
    /// veranderen.
    /// </summary>
    public static class UpgradeCatalog
    {
        public static List<Upgrade> CreateDefault()
        {
            return new List<Upgrade>
            {
                new Upgrade
                {
                    Id = "snellere-vingers",
                    Name = "Snellere vingers",
                    Description = "Verhoogt de waarde van je klik.",
                    Cost = 10,
                    EffectType = UpgradeEffectType.ClickValue,
                    EffectAmount = 0.5
                },
                new Upgrade
                {
                    Id = "extra-medewerker",
                    Name = "Extra medewerker",
                    Description = "Verhoogt je inkomen per seconde.",
                    Cost = 25,
                    EffectType = UpgradeEffectType.IncomePerSecond,
                    EffectAmount = 1.0
                },
                new Upgrade
                {
                    Id = "efficiente-tools",
                    Name = "Efficiënte tools",
                    Description = "Verhoogt de waarde van je klik nog verder.",
                    Cost = 100,
                    EffectType = UpgradeEffectType.ClickValue,
                    EffectAmount = 1.0,
                    RequiresUpgradeId = "snellere-vingers"
                },
                new Upgrade
                {
                    Id = "automatisering-boost",
                    Name = "Automatisering-boost",
                    Description = "Verhoogt je inkomen per seconde flink.",
                    Cost = 250,
                    EffectType = UpgradeEffectType.IncomePerSecond,
                    EffectAmount = 5.0,
                    RequiresUpgradeId = "extra-medewerker"
                }
            };
        }
    }
}
