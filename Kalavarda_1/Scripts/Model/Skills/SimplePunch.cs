using System;
using Assets.Scripts.Utils;
using JetBrains.Annotations;

namespace Assets.Scripts.Model.Skills
{
    [AudioSource("SimplePunch")]
    [Animation(AnimationState.SimplePunch)]
    public class SimplePunch : ISkill
    {
        private readonly IWeapon _weapon;
        private DateTime _lastUseTime = DateTime.MinValue;

        public string Name => "Простой удар";

        public string Description
        {
            get
            {
                if (_weapon is Fist)
                    return "Бьёт кулаком";
                else
                    throw new NotImplementedException();
            }
        }

        public TimeSpan Interval { get; }

        public TimeSpan Cooldown
        {
            get
            {
                if (_lastUseTime + Interval < DateTime.Now)
                    return TimeSpan.Zero;

                return _lastUseTime + Interval - DateTime.Now;
            }
        }

        public bool ReadyToUse => Cooldown == TimeSpan.Zero;

        public float CooldownNormalized => (float)Cooldown.TotalSeconds / (float)Interval.TotalSeconds;

        public SimplePunch([NotNull] IWeapon weapon)
        {
            _weapon = weapon ?? throw new ArgumentNullException(nameof(weapon));
            Interval = TimeSpan.FromSeconds(1);
        }

        public void Use(IHealth target, IHealth source, float distance, Action onStartUse)
        {
            if (Cooldown > TimeSpan.Zero)
                return;

            if (target == null)
                return;

            if (_weapon.MaxDistance < distance)
                return;

            _lastUseTime = DateTime.Now;
            onStartUse?.Invoke();

            var ratio = ((ISkilled)source).GetSkillPower(this);
            target.ChangeHP(-ratio * _weapon.Power, source, this);
        }
    }
}
