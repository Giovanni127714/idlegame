namespace Idlegame.Models
{
    /// <summary>
    /// Eenmalig ontgrendelbare automatisering die daarna zelfstandig, op een
    /// eigen interval, valuta produceert zonder dat de speler hoeft te
    /// klikken.
    /// </summary>
    public class Automation
    {
        public required string Id { get; init; }

        public required string Name { get; init; }

        public required string Description { get; init; }

        public double Cost { get; init; }

        public double ProductionPerTick { get; init; }

        public double IntervalSeconds { get; init; }

        public bool IsUnlocked { get; set; }
    }
}
