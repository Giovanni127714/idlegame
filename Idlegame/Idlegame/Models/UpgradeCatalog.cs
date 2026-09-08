using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Idlegame.Services;

namespace Idlegame.Models
{
    /// <summary>
    /// Bron van de beschikbare upgrades: leest Config/upgrades.json, zodat
    /// upgrades aangepast kunnen worden zonder te hercompileren. Bij een
    /// ontbrekend of corrupt bestand valt dit terug op een kleine ingebouwde
    /// standaardset, net als de veilige-default-aanpak bij een corrupt
    /// savebestand.
    /// </summary>
    public static class UpgradeCatalog
    {
        public static bool TryLoad(out List<Upgrade> upgrades, out string? error)
        {
            try
            {
                upgrades = ContentLoader.LoadList<Upgrade>("upgrades.json");
                error = null;
                return true;
            }
            catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
            {
                upgrades = CreateFallbackDefaults();
                error = ex.Message;
                return false;
            }
        }

        private static List<Upgrade> CreateFallbackDefaults()
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
                }
            };
        }
    }
}
