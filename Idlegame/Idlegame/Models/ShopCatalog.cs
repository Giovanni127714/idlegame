using System.Collections.Generic;

namespace Idlegame.Models
{
    /// <summary>
    /// Bron van de beschikbare winkel-items. Voorlopig hardcoded, zoals de
    /// andere catalogi.
    /// </summary>
    public static class ShopCatalog
    {
        public static List<ShopItem> CreateDefault()
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
                },
                new ShopItem
                {
                    Id = "foodtruck",
                    Name = "Foodtruck",
                    Description = "Rijdt rond en verkoopt aan meer klanten tegelijk.",
                    BaseCost = 100,
                    CostMultiplier = 1.15,
                    IncomePerSecondPerUnit = 1.0
                },
                new ShopItem
                {
                    Id = "fabriek",
                    Name = "Fabriek",
                    Description = "Grootschalige productie voor de late game.",
                    BaseCost = 1100,
                    CostMultiplier = 1.15,
                    IncomePerSecondPerUnit = 8.0
                }
            };
        }
    }
}
