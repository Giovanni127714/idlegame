using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Idlegame.Services
{
    /// <summary>
    /// Leest spelinhoud (upgrades, automatiseringen, winkel-items) uit
    /// JSON-configbestanden onder Config/, zodat deze aangepast kunnen
    /// worden zonder de code te hercompileren.
    /// </summary>
    public static class ContentLoader
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static List<T> LoadList<T>(string fileName)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Config", fileName);
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<T>>(json, JsonOptions)
                ?? throw new JsonException($"Config-bestand '{fileName}' bevat geen geldige lijst.");
        }
    }
}
