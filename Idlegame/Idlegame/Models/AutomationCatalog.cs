using System.Collections.Generic;

namespace Idlegame.Models
{
    /// <summary>
    /// Bron van de beschikbare automatiseringen. Voorlopig hardcoded, zoals
    /// UpgradeCatalog.
    /// </summary>
    public static class AutomationCatalog
    {
        public static List<Automation> CreateDefault()
        {
            return new List<Automation>
            {
                new Automation
                {
                    Id = "auto-klikker",
                    Name = "Auto-klikker",
                    Description = "Klikt automatisch voor je, zodat je zelf niet meer hoeft te klikken.",
                    Cost = 50,
                    ProductionPerTick = 1.0,
                    IntervalSeconds = 3.0
                },
                new Automation
                {
                    Id = "robotarm",
                    Name = "Robotarm",
                    Description = "Een krachtige arm die in hoog tempo grote hoeveelheden produceert.",
                    Cost = 300,
                    ProductionPerTick = 5.0,
                    IntervalSeconds = 4.0
                }
            };
        }
    }
}
