using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Idlegame.Services;

namespace Idlegame.Models
{
    /// <summary>
    /// Bron van de beschikbare winkel-items: leest Config/shop.json, met
    /// dezelfde fallback-aanpak als UpgradeCatalog bij een ontbrekend of
    /// corrupt bestand.
    /// </summary>
    public static class ShopCatalog
    {
        public static bool TryLoad(out List<ShopItem> shopItems, out string? error)
        {
            try
            {
                shopItems = ContentLoader.LoadList<ShopItem>("shop.json");
                error = null;
                return true;
            }
            catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
            {
                shopItems = CreateFallbackDefaults();
                error = ex.Message;
                return false;
            }
        }

        private static List<ShopItem> CreateFallbackDefaults()
        {
            return new List<ShopItem>
            {
                new ShopItem
                {
                    Id = "limonadekraam",
                    Name = "Limonadekraampje",
                    Description = "Een klein kraampje dat gestaag valuta binnenbrengt.",
                    BaseCost = 15,
                    CostMultiplier = 1.15,
                    IncomePerSecondPerUnit = 0.1
                }
            };
        }
    }
}
