using System;
using Assets.Scripts.Utils;
using JetBrains.Annotations;

namespace Assets.Scripts.Model.Skills
{
    [AudioSource("SimplePunch")]
    [Animation(AnimationState.SimplePunch)]
    public class SimplePunch : ISkill
    {
        private readonly ISkilled _source;
        private readonly IWeapon _weapon;
        private DateTime _lastUseTime = DateTime.MinValue;

        public string Name => "Простой удар";

        public string Description
        {
            get
            {
                if (_weapon is Fist) // костыльненько
                    return $"Бьёт кулаком с силой {Math.Round(_source.GetSkillPower(this, SkillContext.Empty), 1)}";
                else
                    return $"Используя {_weapon}, наносит удар с силой {Math.Round(_source.GetSkillPower(this, SkillContext.Empty), 1)}";
            }
        }

        public TimeSpan Interval => TimeSpan.FromSeconds(1);

        public TimeSpan Cooldown
        {
            get
            {
                if (_lastUseTime + Interval < DateTime.Now)
                    return TimeSpan.Zero;

                return _lastUseTime + Interval - DateTime.Now;
            }
        }

        public bool ReadyToUse(SkillContext context)
        {
            return Cooldown == TimeSpan.Zero && context.Target != null && context.Distance < MaxDistance;
        }

        public float CooldownNormalized => (float)Cooldown.TotalSeconds / (float)Interval.TotalSeconds;

        public float MaxDistance => _weapon.MaxDistance;

        public SimplePunch([NotNull] ISkilled source, [NotNull] IWeapon weapon)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _weapon = weapon ?? throw new ArgumentNullException(nameof(weapon));
        }

        public void Use(SkillContext context, Action onStartUse)
        {
            if (!ReadyToUse(context))
                return;

            _lastUseTime = DateTime.Now;
            onStartUse?.Invoke();

            var ratio = _source.GetSkillPower(this, context);

            context.Target.ChangeHP(-ratio * _weapon.Power, _source, this);
        }
    }
}
