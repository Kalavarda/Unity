using System;

namespace Assets.Scripts.Model
{
    public interface IBuff
    {
        string Name { get; }

        string Description { get; }

        bool IsNegitive { get; }

        /// <summary>
        /// До какого момента действует
        /// </summary>
        DateTime EndTime { get; }
    }

    public interface IDotBuff
    {
        TimeSpan HealthInterval { get; }
    }
}
