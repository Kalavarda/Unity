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

        bool ReadyToUse(IHealth target, float distance);

        void Use(IHealth target, IHealth source, float distance, Action onStartUse);
    }

    public interface ICastableSkill
    {
        TimeSpan MinCastDuration { get; }

        TimeSpan MaxCastDuration { get; }

        TimeSpan? CastDuration { get; }

        float? CastDurationNormalized { get; }

        void BeginCast();

        void EndCast();

        bool IsCasting { get; }

        event Action<ICastableSkill> OnBeginCast;

        event Action<ICastableSkill> OnEndCast;
    }

    public interface ISkilled
    {
        IReadOnlyCollection<ISkill> Skills { get; }

        /// <summary>
        /// Множитель силы при использовании скила
        /// </summary>
        float GetSkillPower(ISkill skill);

        void Use(ISkill skill, IHealth target, float distance, Action onStartUse);
    }

    public interface IThrowingSkill
    {
        void OnOnCollisionEnter(IHealth health, float velocity);

        /// <summary>
        /// Предмет, который надо бросить
        /// </summary>
        IThing Thing { get; }
    }
}
