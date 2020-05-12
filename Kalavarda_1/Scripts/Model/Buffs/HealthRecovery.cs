using System;
using Assets.Scripts.Utils;
using JetBrains.Annotations;

namespace Assets.Scripts.Model.Buffs
{
    public class HealthRecovery: IBuff, IDotBuff, IModifierCorrector
    {
        private readonly ISkilled _source;
        private readonly IHealth _target;
        private readonly ISkill _skill;
        private readonly float _health;
        private readonly TimeIntervalLimiter _limiter = new TimeIntervalLimiter(TimeSpan.FromSeconds(1f));

        public void Affect(IWritableModifier modifier)
        {
            _limiter.Do(() =>
            {
                _target.ChangeHP(_health, _source, _skill);
            });
        }

        public string Name => "Восстановление здоровья";

        public string Description =>
            $"Восстанавливает {Math.Round(_health, 1)} здоровья c периодичностью {Utils.Utils.ToString(_limiter.Interval)}";

        public bool IsNegitive => false;

        public DateTime EndTime { get; }

        public TimeSpan HealthInterval => _limiter.Interval;

        public HealthRecovery([NotNull] ISkilled source, [NotNull] IHealth target, [NotNull] ISkill skill, DateTime endTime, float health)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _skill = skill ?? throw new ArgumentNullException(nameof(skill));
            _health = health;
            EndTime = endTime;
        }
    }
}
