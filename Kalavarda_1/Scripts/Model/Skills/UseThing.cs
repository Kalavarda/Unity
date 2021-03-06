﻿using System;
using Assets.Scripts.Model.Buffs;
using Assets.Scripts.Model.Things;
using JetBrains.Annotations;

namespace Assets.Scripts.Model.Skills
{
    public class UseThing: ISkill, ICastableSkill
    {
        private readonly ISkilled _source;
        private SkillContext _skillContext;
        private DateTime _lastUseTime = DateTime.MinValue;
        private DateTime? _startCastTime;
        private IThing _lastUsedThing;

        public IThing Thing { get; set; }

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

        public bool CanUseInFight
        {
            get
            {
                if (Thing is Chest)
                    return false;

                return true;
            }
        }

        public UseThing([NotNull] ISkilled source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public bool ReadyToUse(SkillContext context)
        {
            if (CastingInProgress)
                return false;

            var baseConditions = Cooldown == TimeSpan.Zero && context.Target != null && context.Distance < MaxDistance;
            if (!baseConditions)
                return false;

            return true;
        }

        public void Use(SkillContext context)
        {
            if (!ReadyToUse(context))
                return;

            _skillContext = context;
            _lastUsedThing = null;

            _startCastTime = DateTime.Now;
            OnBeginCast?.Invoke(this);
        }

        public event Action<ISkill, SkillContext> OnSuccessUsed;

        private void Use(IHealth target, IThing thing)
        {
            if (thing is IEquipment equipment)
                if (target is Player player)
                {
                    player.PutOn(equipment);
                    _lastUseTime = DateTime.Now;
                    return;
                }

            if (thing is IStack stack)
            {
                // костыльно
                if (stack.Prototype == MeatPrototype.Instance)
                    if (target is Player player)
                    {
                        var ratio = player.GetSkillPower(this, _skillContext);
                        var buff = new HealthRecovery(_source, target, this, DateTime.Now.AddSeconds(20), 1 * ratio);
                        player.AddBuff(buff);
                    }

                _lastUseTime = DateTime.Now;
                return;
            }

            if (thing is ILoot loot)
                if (_source is Player player)
                {
                    foreach (var thing2 in loot.GetLoot())
                        player.Bag.Add(thing2);

                    _lastUseTime = DateTime.Now;
                    return;
                }

            if (thing is Recipe recipe)
                if (_source is Player player)
                {
                    player.Recipes.Add(recipe);
                    player.Bag.Pull(thing);

                    _lastUseTime = DateTime.Now;
                    return;
                }

            throw new NotImplementedException();
        }

        public event Action<ICastableSkill> OnBeginCast;

        public bool CastingInProgress => _startCastTime != null;

        public event Action<ICastableSkill> OnEndCast;
        
        public TimeSpan CastDuration
        {
            get
            {
                if (Thing is Chest)
                    return TimeSpan.FromSeconds(3);

                if (Thing?.Prototype is MeatPrototype)
                    return TimeSpan.FromSeconds(5);

                return TimeSpan.FromSeconds(1);
            }
        }

        private TimeSpan CastedDuration
        {
            get
            {
                if (_startCastTime == null)
                    throw new NotImplementedException();

                var elapsed = DateTime.Now - _startCastTime.Value;

                if (_lastUsedThing != Thing)
                    if (elapsed >= CastDuration)
                    {
                        _lastUsedThing = Thing;
                        Use(_skillContext.Target, Thing);

                        OnSuccessUsed?.Invoke(this, _skillContext);
                        OnEndCast?.Invoke(this);
                        _startCastTime = null;
                        _skillContext = null;
                    }

                return elapsed;
            }
        }

        public float CastedDurationNormalized => (float)CastedDuration.TotalSeconds / (float)CastDuration.TotalSeconds;
    }
}
