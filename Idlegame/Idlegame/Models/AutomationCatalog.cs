using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Idlegame.Services;

namespace Idlegame.Models
{
    /// <summary>
    /// Bron van de beschikbare automatiseringen: leest Config/automations.json,
    /// met dezelfde fallback-aanpak als UpgradeCatalog bij een ontbrekend of
    /// corrupt bestand.
    /// </summary>
    public static class AutomationCatalog
    {
        public static bool TryLoad(out List<Automation> automations, out string? error)
        {
            try
            {
                automations = ContentLoader.LoadList<Automation>("automations.json");
                error = null;
                return true;
            }
            catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
            {
                automations = CreateFallbackDefaults();
                error = ex.Message;
                return false;
            }
        }

        private static List<Automation> CreateFallbackDefaults()
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
                }
            };
        }
    }
}
