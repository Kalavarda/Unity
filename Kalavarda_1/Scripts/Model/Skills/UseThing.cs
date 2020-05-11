using System;
using Assets.Scripts.Model.Buffs;
using JetBrains.Annotations;

namespace Assets.Scripts.Model.Skills
{
    public class UseThing: ISkill
    {
        private readonly IThing _thing;
        private DateTime _lastUseTime = DateTime.MinValue;

        public string Name => "Использовать " + _thing.Prototype.Name;

        public string Description => $"Используется предмет '{_thing.Prototype.Name}'";

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

        public float CooldownNormalized => (float)Cooldown.TotalSeconds / (float)Interval.TotalSeconds;

        public float MaxDistance => 1.55f;

        public UseThing([NotNull] IThing thing)
        {
            _thing = thing ?? throw new ArgumentNullException(nameof(thing));
        }

        public bool ReadyToUse(IHealth target, float distance)
        {
            var baseConditions = Cooldown == TimeSpan.Zero && target != null && distance < MaxDistance;
            if (!baseConditions)
                return false;

            if (target is IFighter fighter)
                if (fighter.InFight)
                {
                    if (_thing.Prototype == MeatPrototype.Instance)
                        return false;
                }

            return true;
        }

        public void Use(IHealth target, IHealth source, float distance, Action onStartUse)
        {
            if (!ReadyToUse(target, distance))
                return;

            if (_thing is IEquipment equipment)
                if (target is Player player)
                {
                    player.PutOn(equipment);
                    _lastUseTime = DateTime.Now;
                    onStartUse?.Invoke();
                    return;
                }

            if (_thing is IStack stack)
            {
                // костыльно
                if (stack.Prototype == MeatPrototype.Instance)
                    if (target is Player player)
                    {
                        var ratio = player.GetSkillPower(this);
                        var buff = new HealthRecovery(source, target, this, DateTime.Now.AddSeconds(10), 2 * ratio);
                        player.AddBuff(buff);
                    }

                _lastUseTime = DateTime.Now;
                onStartUse?.Invoke();
                return;
            }

            throw new NotImplementedException();
        }
    }
}
