using System;
using Assets.Scripts.Model.Buffs;
using JetBrains.Annotations;

namespace Assets.Scripts.Model.Skills
{
    public class UseThing: ISkill
    {
        private readonly ISkilled _source;
        private DateTime _lastUseTime = DateTime.MinValue;

        public IThing Thing { get; }

        public string Name => "Использовать " + Thing.Prototype.Name;

        public string Description => $"Используется предмет '{Thing.Prototype.Name}'";

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

        public UseThing([NotNull] ISkilled source, [NotNull] IThing thing)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            Thing = thing ?? throw new ArgumentNullException(nameof(thing));
        }

        public bool ReadyToUse(IHealth target, float distance)
        {
            var baseConditions = Cooldown == TimeSpan.Zero && target != null && distance < MaxDistance;
            if (!baseConditions)
                return false;

            if (target is IFighter fighter)
                if (fighter.InFight)
                {
                    if (Thing.Prototype == MeatPrototype.Instance)
                        return false;
                }

            return true;
        }

        public void Use(IHealth target, float distance, Action onStartUse)
        {
            if (!ReadyToUse(target, distance))
                return;

            if (Thing is IEquipment equipment)
                if (target is Player player)
                {
                    player.PutOn(equipment);
                    _lastUseTime = DateTime.Now;
                    onStartUse?.Invoke();
                    return;
                }

            if (Thing is IStack stack)
            {
                // костыльно
                if (stack.Prototype == MeatPrototype.Instance)
                    if (target is Player player)
                    {
                        var ratio = player.GetSkillPower(this);
                        var buff = new HealthRecovery(_source, target, this, DateTime.Now.AddSeconds(10), 2 * ratio);
                        player.AddBuff(buff);
                    }

                _lastUseTime = DateTime.Now;
                onStartUse?.Invoke();
                return;
            }

            if (Thing is ILoot loot)
                if (_source is Player player)
                {
                    foreach (var thing in loot.GetLoot())
                        player.Bag.Add(thing);

                    _lastUseTime = DateTime.Now;
                    onStartUse?.Invoke();
                    return;
                }

            if (Thing is Recipe recipe)
                if (_source is Player player)
                {
                    player.Recipes.Add(recipe);
                    player.Bag.Pull(Thing);

                    _lastUseTime = DateTime.Now;
                    onStartUse?.Invoke();
                    return;
                }

            throw new NotImplementedException();
        }
    }
}
