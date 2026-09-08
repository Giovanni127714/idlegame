using System;

namespace Idlegame.Models
{
    /// <summary>
    /// Herhaaldelijk aankoopbaar winkel-item ("producer"). Elke aankoop
    /// verhoogt het aantal bezeten stuks en daarmee de doorlopende
    /// inkomen/sec-bijdrage; de prijs schaalt exponentieel met het aantal
    /// al aangeschafte stuks (prijs = base * multiplier^aantal).
    /// </summary>
    public class ShopItem
    {
        public required string Id { get; init; }

        public required string Name { get; init; }

        public required string Description { get; init; }

        public double BaseCost { get; init; }

        public double CostMultiplier { get; init; } = 1.15;

        public double IncomePerSecondPerUnit { get; init; }

        public int Quantity { get; set; }

        public double CurrentCost => BaseCost * Math.Pow(CostMultiplier, Quantity);
    }
}
