using System;

namespace Assets.Scripts.Model
{
    public interface IFighter
    {
        /// <summary>
        /// В бою
        /// </summary>
        bool InFight { get; }

        event Action<IFighter> FightBegin;

        event Action<IFighter> FightEnd;

        event Action<DamageInfo> DagameReceived;
    }
}
