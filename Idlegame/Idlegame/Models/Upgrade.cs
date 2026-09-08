namespace Idlegame.Models
{
    /// <summary>
    /// Eenmalig aankoopbare upgrade met een vast effect. Optioneel afhankelijk
    /// van een andere upgrade (RequiresUpgradeId) die eerst gekocht moet zijn.
    /// </summary>
    public class Upgrade
    {
        public required string Id { get; init; }

        public required string Name { get; init; }

        public required string Description { get; init; }

        public double Cost { get; init; }

        public UpgradeEffectType EffectType { get; init; }

        public double EffectAmount { get; init; }

        public string? RequiresUpgradeId { get; init; }

        public bool IsPurchased { get; set; }
    }
}
