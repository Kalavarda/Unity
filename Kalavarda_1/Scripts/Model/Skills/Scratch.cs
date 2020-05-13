using System;
using Assets.Scripts.Model.Buffs;
using JetBrains.Annotations;

namespace Assets.Scripts.Model.Skills
{
    public class Scratch: ISkill
    {
        private readonly ISkilled _source;
        private const float BaseDamage = 0.5f;
        private DateTime _lastUseTime = DateTime.MinValue;
        private static readonly TimeSpan DebuffDuration = TimeSpan.FromSeconds(10);

        public string Name => "Поцарапать";

        public string Description =>
            $"Царапает противника, оставляет на противнике эффект Кровотечение на {DebuffDuration.TotalSeconds} секунд.";

        public TimeSpan Interval => TimeSpan.FromSeconds(15);

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

        public float MaxDistance => 1.55f;

        public Scratch([NotNull] ISkilled source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public void Use(SkillContext context, Action onStartUse)
        {
            if (context.Distance > MaxDistance)
                return;

            if (context.Target is Player player)
            {
                var ratio = 1f;
                if (_source is ISkilled skilled)
                    ratio = skilled.GetSkillPower(this, context);

                var debuff = new Bleeding(_source, context.Target, this, DateTime.Now + DebuffDuration, BaseDamage * ratio);
                player.AddBuff(debuff);
                _lastUseTime = DateTime.Now;
                return;
            }

            throw new NotImplementedException();
        }
    }
}
