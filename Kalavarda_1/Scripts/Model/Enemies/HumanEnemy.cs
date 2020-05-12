﻿using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model.Skills;
using Assets.Scripts.Utils;
using JetBrains.Annotations;

namespace Assets.Scripts.Model.Enemies
{
    public class HumanEnemy : IHealth, ILoot, ISpawned, ISkilled, IEnemy, IFighter
    {
        private readonly IReadOnlyCollection<IThing> _loot;
        private readonly TimeIntervalLimiter _autoHealthIncrease = new TimeIntervalLimiter(TimeSpan.FromSeconds(1));
        private bool _inFight;
        private readonly ISkill[] _skills;
        private DateTime _lastFightTime = DateTime.MinValue;
        private static readonly TimeSpan FightExitTime = TimeSpan.FromSeconds(10);
        private readonly IModifierCorrector _corrector;
        private readonly List<IBuff> _buffs = new List<IBuff>();

        public bool IsFemale { get; }

        public EnemyCharacteristics Characteristics { get; }

        public HumanEnemy(float hpRatio, float powerRatio, [NotNull] IReadOnlyCollection<IThing> loot, bool isFemale = false)
        {
            _loot = loot ?? throw new ArgumentNullException(nameof(loot));
            Characteristics = new EnemyCharacteristics();
            _corrector = new EnemyCorrector(hpRatio, powerRatio);
            HP = Characteristics.MaxHP.Value;
            IsFemale = isFemale;

            var skills = new List<ISkill> { new SimplePunch(this, new Fist()) };
            if (isFemale)
                skills.Add(new Scratch(this));
            _skills = skills.ToArray();
        }

        public IReadOnlyCollection<IThing> GetLoot()
        {
            return _loot;
        }

        public IReadOnlyCollection<ISkill> Skills => _skills;

        public float HP { get; private set; }

        public float HPNormalized => HP / Characteristics.MaxHP.Value;

        public float MaxHP => Characteristics.MaxHP.Value;

        public bool IsDied => HP <= 0;

        public bool InFight
        {
            get => _inFight;
            private set
            {
                var oldValue = InFight;
                _inFight = value;
                if (!oldValue && value)
                    FightBegin?.Invoke(this);
                if (oldValue && !value)
                    FightEnd?.Invoke(this);
            }
        }

        public IReadOnlyCollection<IBuff> Buffs
        {
            get
            {
                if (IsDied)
                    return new IBuff[0];

                return _buffs;
            }
        }

        public string Name => IsFemale ? "Злая баба" : "Злой мужик";

        public float AggressionDistance => 25;

        public event Action<IFighter> FightBegin;
        public event Action<IFighter> FightEnd;

        public event Action<DamageInfo> DagameReceived;

        public void ChangeHP(float hpChange, ISkilled source, ISkill skill)
        {
            if (source != null && source != this && hpChange < 0)
            {
                _lastFightTime = DateTime.Now;
                InFight = true;
                AggressionTarget = (IHealth)source;
            }

            HP = Math.Min(Math.Max(0, HP + hpChange), Characteristics.MaxHP.Value);
            if (source != null && skill != null && hpChange < 0)
                DagameReceived?.Invoke(new DamageInfo(source, this, skill, -hpChange));

            if (HP <= 0)
                Died?.Invoke(this);
        }

        public event Action<IHealth> Died;

        public float GetSkillPower(ISkill skill)
        {
            var r = Characteristics.PowerRatio.Value;

            r *= 0.5f + HPNormalized / 2;

            return r;
        }

        public virtual void Update()
        {
            _autoHealthIncrease.Do(() =>
            {
                ChangeHP(Characteristics.HPRecoveryRatio.Value * Characteristics.MaxHP.Value, this, null);
            });

            if (DateTime.Now - _lastFightTime > FightExitTime)
                InFight = false;

            if (_buffs.Any())
                foreach (var buff in _buffs.Where(b => DateTime.Now > b.EndTime).ToArray())
                    _buffs.Remove(buff);

            Characteristics.Reset();

            ModifyCharacteristics(_corrector);
            foreach (var buff in _buffs)
                if (buff is IModifierCorrector corrector)
                    ModifyCharacteristics(corrector);
        }

        private void ModifyCharacteristics(IModifierCorrector corrector)
        {
            foreach (var modifier in Characteristics.AllModifiers)
                if (modifier is IWritableModifier wrModifier)
                    corrector.Affect(wrModifier);
        }

        public IHealth AggressionTarget { get; set; }

        public void AddBuff(IBuff buff)
        {
            if (buff == null) throw new ArgumentNullException(nameof(buff));
            _buffs.Add(buff);
        }

        public void Use([NotNull] ISkill skill, IHealth target, float distance, Action onStartUse)
        {
            if (skill == null) throw new ArgumentNullException(nameof(skill));

            if (!skill.ReadyToUse(target, distance))
                return;

            skill.Use(target, distance, () =>
            {
                if (target != null && target != this)
                {
                    _lastFightTime = DateTime.Now;
                    InFight = true;
                }
                onStartUse?.Invoke();
                OnUseSkill?.Invoke(this, skill);
            });
        }

        public event Action<ISkilled, ISkill> OnUseSkill;
    }

    public class HumanToothPrototype : IThingPrototype
    {
        public static HumanToothPrototype Instance { get; } = new HumanToothPrototype();

        public string Name => "Зуб человека";

        public int BagMaxStackCount => 100;

        public IThing CreateInstance()
        {
            return new Stack(Instance);
        }
    }

    public class UnderwearPrototype : IThingPrototype
    {
        public static UnderwearPrototype Instance { get; } = new UnderwearPrototype();

        public string Name => "Нижнее бельё";

        public int BagMaxStackCount => 10;

        public IThing CreateInstance()
        {
            return new Stack(Instance);
        }
    }

    public class ScalpPrototype : IThingPrototype
    {
        public static ScalpPrototype Instance { get; } = new ScalpPrototype();

        public string Name => "Скальп человека";

        public int BagMaxStackCount => 10;

        public IThing CreateInstance()
        {
            return new Stack(Instance);
        }
    }

    public class PantsPrototype : IThingPrototype
    {
        public static PantsPrototype Instance { get; } = new PantsPrototype();

        public string Name => "Штаны";

        public int BagMaxStackCount => 10;

        public IThing CreateInstance()
        {
            return new Stack(Instance);
        }
    }
}
