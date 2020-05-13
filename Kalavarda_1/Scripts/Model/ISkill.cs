using System;
using System.Collections.Generic;

namespace Assets.Scripts.Model
{
    public interface ISkill
    {
        string Name { get; }

        string Description { get; }

        TimeSpan Interval { get; }

        TimeSpan Cooldown { get; }

        float CooldownNormalized { get; }

        float MaxDistance { get; }

        bool ReadyToUse(SkillContext context);

        void Use(SkillContext context, Action onStartUse);
    }

    public interface ICastableSkill
    {
        event Action<ICastableSkill> OnBeginCast;

        event Action<ICastableSkill> OnEndCast;

        /// <summary>
        /// Время, необходимое на каст
        /// </summary>
        TimeSpan CastDuration { get; }

        /// <summary>
        /// Сколько уже скастовалось
        /// </summary>
        float CastedDurationNormalized { get; }

        /// <summary>
        /// В данный момент идёт подготовка
        /// </summary>
        bool CastingInProgress { get; }
    }

    public interface ISkilled
    {
        IReadOnlyCollection<ISkill> Skills { get; }

        /// <summary>
        /// Множитель силы при использовании скила
        /// </summary>
        float GetSkillPower(ISkill skill, SkillContext context);

        void Use(ISkill skill, SkillContext context, Action onStartUse = null);

        /// <summary>
        /// Вызывается перед тем, как начнётся применение умения
        /// </summary>
        event Action<ISkilled, ISkill> BeforeUseSkill;
    }

    public class SkillContext
    {
        public static SkillContext Empty { get; } = new SkillContext(null, 0, 0);

        public float Distance { get; }

        public IHealth Target { get; }

        /// <summary>
        /// Положение по отношению к цели (0 - сзади, 0.5 - сбоку, 1 - спереди)
        /// </summary>
        public float Angle { get; }

        public SkillContext(IHealth target, float distance, float angle)
        {
            Target = target;
            Distance = distance;
            Angle = angle;
        }
    }
}
