using System;
using Assets.Scripts.Utils;
using JetBrains.Annotations;

namespace Assets.Scripts.Model.Buffs
{
    public class Bleeding: IBuff, IDotBuff, IModifierCorrector
    {
        private readonly IHealth _source;
        private readonly IHealth _target;
        private readonly ISkill _skill;
        private readonly float _damage;
        private readonly TimeIntervalLimiter _limiter = new TimeIntervalLimiter(TimeSpan.FromSeconds(1f));

        public void Affect(IWritableModifier modifier)
        {
            _limiter.Do(() =>
            {
                _target.ChangeHP(-_damage, _source, _skill);
            });
        }

        public string Name => "Кровотечение";

        public string Description =>
            $"Наносит {Math.Round(_damage, 1)} урона c периодичностью {Utils.Utils.ToString(_limiter.Interval)}";

        public bool IsNegitive => true;
        
        public DateTime EndTime { get; }

        public TimeSpan HealthInterval => _limiter.Interval;

        public Bleeding([NotNull] IHealth source, [NotNull] IHealth target, [NotNull] ISkill skill, DateTime endTime, float damage)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _skill = skill ?? throw new ArgumentNullException(nameof(skill));
            _damage = damage;
            EndTime = endTime;
        }
    }
}
