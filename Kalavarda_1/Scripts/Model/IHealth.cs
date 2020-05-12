using System;

namespace Assets.Scripts.Model
{
    public interface IHealth
    {
        float HP { get; }

        float HPNormalized { get; }

        float MaxHP { get; }

        bool IsDied { get; }

        void ChangeHP(float hpChange, ISkilled source, ISkill skill);

        event Action<IHealth> Died;

        /// <summary>
        /// вызывается постоянно и часто, в любой момент
        /// </summary>
        void Update();
    }
}
