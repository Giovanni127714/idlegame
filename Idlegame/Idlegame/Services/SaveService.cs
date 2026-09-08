using System;
using System.IO;
using System.Text.Json;
using Idlegame.Models;

namespace Idlegame.Services
{
    /// <summary>
    /// Leest en schrijft SaveData als JSON in de lokale AppData-map van de
    /// gebruiker. Schrijft eerst naar een tijdelijk bestand en verplaatst dat
    /// pas over het echte savebestand, zodat een crash of stroomstoring
    /// halverwege het schrijven nooit een corrupt savebestand achterlaat.
    /// </summary>
    public class SaveService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        private readonly string _saveFilePath;

        public SaveService()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Idlegame");
            Directory.CreateDirectory(folder);
            _saveFilePath = Path.Combine(folder, "save.json");
        }

        public void Save(SaveData data)
        {
            string json = JsonSerializer.Serialize(data, JsonOptions);

            string tempFilePath = _saveFilePath + ".tmp";
            File.WriteAllText(tempFilePath, json);
            File.Move(tempFilePath, _saveFilePath, overwrite: true);
        }
    }
}
