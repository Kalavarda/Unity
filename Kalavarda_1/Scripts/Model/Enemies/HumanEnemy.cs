using System;
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
        private readonly DefenceCorrector _defenceCorrector;

        public bool IsFemale { get; }

        public EnemyCharacteristics Characteristics { get; }

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

        public HumanEnemy(float hpRatio, float powerRatio, [NotNull] IReadOnlyCollection<IThing> loot, bool isFemale = false)
        {
            _loot = loot ?? throw new ArgumentNullException(nameof(loot));
            Characteristics = new EnemyCharacteristics();
            _corrector = new EnemyCorrector(hpRatio, powerRatio);
            _defenceCorrector = new DefenceCorrector(this);
            HP = Characteristics.MaxHP.Value;
            IsFemale = isFemale;

            var skills = new List<ISkill> { new SimplePunch(this, new Fist()) };
            if (isFemale)
                skills.Add(new Scratch(this));
            _skills = skills.ToArray();

            foreach (var skill in Skills)
                skill.OnSuccessUsed += Skill_OnSuccessUsed;
        }

        private void Skill_OnSuccessUsed(ISkill skill, SkillContext context)
        {
            if (context.Target == null || context.Target == this)
                return;

            _lastFightTime = DateTime.Now;
            InFight = true;
        }

        public void ChangeHP(float hpChange, ISkilled source, ISkill skill)
        {
            if (source != null && source != this && hpChange < 0)
            {
                _lastFightTime = DateTime.Now;
                InFight = true;
                AggressionTarget = (IHealth)source;
            }

            if (hpChange < 0)
                hpChange /= Characteristics.DefenceRatio.Value;

            HP = Math.Min(Math.Max(0, HP + hpChange), Characteristics.MaxHP.Value);
            if (source != null && skill != null && hpChange < 0)
                DagameReceived?.Invoke(new DamageInfo(source, this, skill, -hpChange));

            if (HP <= 0)
                Died?.Invoke(this);
        }

        public event Action<IHealth> Died;

        public float GetSkillPower(ISkill skill, SkillContext context)
        {
            var r = Characteristics.PowerRatio.Value;

            r *= 0.5f + HPNormalized / 2;
            if (context.Angle != null)
                r *= 2 - context.Angle.Value; // х2 при ударе в спину

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
            ModifyCharacteristics(_defenceCorrector);
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

        public void Use([NotNull] ISkill skill, SkillContext context)
        {
            if (skill == null) throw new ArgumentNullException(nameof(skill));

            if (!skill.CanUseInFight && InFight)
                return;

            if (!skill.ReadyToUse(context))
                return;

            BeforeUseSkill?.Invoke(this, skill);
            skill.Use(context);
        }

        public event Action<ISkilled, ISkill> BeforeUseSkill;
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
