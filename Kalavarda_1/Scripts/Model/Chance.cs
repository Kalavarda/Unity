using System;

namespace Assets.Scripts
{
    public interface IChanceCalculator
    {
        bool Try(float chance);
    }

    public class Chance: IChanceCalculator
    {
        public static Chance Instance { get; } = new Chance();

        private static readonly Random Random = new Random();

        public bool Try(double chance)
        {
            if (chance < 0 || chance > 1)
                throw new ArgumentOutOfRangeException(nameof(chance) + " = " + chance);

            return chance > Random.NextDouble();
        }

        public bool Try(float chance)
        {
            return Try((double)chance);
        }
    }
}
