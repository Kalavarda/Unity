using System;
using Assets.Scripts.Model.Buffs;

namespace Assets.Scripts.Model.Skills
{
    public class Scratch: ISkill
    {
        private const float BaseDamage = 0.5f;
        private DateTime _lastUseTime = DateTime.MinValue;
        private static readonly TimeSpan DebuffDuration = TimeSpan.FromSeconds(10);

        public string Name => "Поцарапать";

        public string Description =>
            $"Царапает противника, оставляет на противнике эффект Кровотечение на {DebuffDuration.TotalSeconds} секунд.";

        public TimeSpan Interval => TimeSpan.FromSeconds(20);

        public TimeSpan Cooldown
        {
            get
            {
                if (_lastUseTime + Interval < DateTime.Now)
                    return TimeSpan.Zero;

                return _lastUseTime + Interval - DateTime.Now;
            }
        }

        public bool ReadyToUse(IHealth target, float distance)
        {
            return Cooldown == TimeSpan.Zero && target != null && distance < MaxDistance;
        }

        public float CooldownNormalized => (float)Cooldown.TotalSeconds / (float)Interval.TotalSeconds;

        public float MaxDistance => 1.55f;

        public void Use(IHealth target, IHealth source, float distance, Action onStartUse)
        {
            if (distance > MaxDistance)
                return;

            if (target is Player player)
            {
                var ratio = 1f;
                if (source is ISkilled skilled)
                    ratio = skilled.GetSkillPower(this);

                var debuff = new Bleeding(source, target, this, DateTime.Now + DebuffDuration, BaseDamage * ratio);
                player.AddBuff(debuff);
                _lastUseTime = DateTime.Now;
                return;
            }

            throw new NotImplementedException();
        }
    }
}
