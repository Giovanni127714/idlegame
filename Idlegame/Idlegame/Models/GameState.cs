namespace Idlegame.Models
{
    public class GameState
    {
        public const double BaseIncomePerSecond = 1.0;
        public const double BaseClickValue = 1.0;

        public double Currency { get; set; }

        /// <summary>Totaal ooit verdiende valuta (nooit verlaagd door aankopen); bepaalt hoeveel prestigepunten beschikbaar zijn.</summary>
        public double TotalEarned { get; set; }

        public double IncomePerSecond { get; set; } = BaseIncomePerSecond;

        public double ClickValue { get; set; } = BaseClickValue;

        /// <summary>Permanente prestigepunten ("sterren"); overleven een prestige-reset.</summary>
        public int PrestigePoints { get; set; }

        /// <summary>Permanente vermenigvuldiger op al het inkomen, gebaseerd op PrestigePoints (+10% per punt).</summary>
        public double PrestigeMultiplier => 1.0 + (PrestigePoints * 0.1);
    }
}
