using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model.Skills;
using Assets.Scripts.Utils;
using JetBrains.Annotations;

namespace Assets.Scripts.Model
{
    public class Player: IHealth, IArmed, ISkilled, IFighter
    {
        private static readonly TimeSpan FightExitTime = TimeSpan.FromSeconds(10);
        private readonly TimeIntervalLimiter _autoHealthIncrease = new TimeIntervalLimiter(TimeSpan.FromSeconds(1));

        private DateTime _lastFightTime = DateTime.MinValue;

        private readonly List<IWeapon> _weapons = new List<IWeapon>();
        private readonly List<ISkill> _skills = new List<ISkill>();
        private bool _inFight;

        private readonly List<IEquipment> _equipments = new List<IEquipment>();
        private readonly List<IBuff> _buffs = new List<IBuff>();

        private readonly Dictionary<Type, float> _skillExperience = new Dictionary<Type, float>();
        private readonly SlowWhileUsingSkill _usingSkillCorrector;
        private readonly DefenceCorrector _defenceCorrector;

        public Cheat CheatModifier { get; } = new Cheat();

        public IReadOnlyCollection<IEquipment> Equipments => _equipments;

        public IReadOnlyCollection<IBuff> Buffs
        {
            get
            {
                if (IsDied)
                    return new IBuff[0];

                return _buffs;
            }
        }

        public PlayerCharacteristics Characteristics { get; }

        public IRecipeCollection Recipes { get; }

        public IBag Bag { get; }

        public static Player Instance { get; } = new Player(new Bag());

        public float HP { get; protected set; }

        public float HPNormalized => HP / MaxHP;

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

        public event Action<IFighter> FightBegin;
        public event Action<IFighter> FightEnd;
        public event Action<DamageInfo> DagameReceived;

        private Player([NotNull] IBag bag)
        {
            Bag = bag ?? throw new ArgumentNullException(nameof(bag));
            Characteristics = new PlayerCharacteristics();
            _usingSkillCorrector = new SlowWhileUsingSkill(this);

            HP = MaxHP;

            var fist = new Fist();
            _weapons.Add(fist);
            _skills.Add(new SimplePunch(this, fist));
            _skills.Add(new UseThing(this));
            _skills.Add(new StrongPunch(this, fist));

            foreach (var skill in Skills)
                skill.OnSuccessUsed += Skill_OnSuccessUsed;

            Recipes = new RecipeCollection();
/*
            Bag.Add(new Stack(ScalpPrototype.Instance, 15));
            Bag.Add(new Stack(UnderwearPrototype.Instance, 15));
            Bag.Add(new Stack(HumanToothPrototype.Instance, 150));
            Bag.Add(new Stack(PantsPrototype.Instance, 15));
            Bag.Add(new Stack(MeatPrototype.Instance, 5));
*/
            _defenceCorrector = new DefenceCorrector(this);
        }

        private void Skill_OnSuccessUsed(ISkill skill, SkillContext context)
        {
            if (context.Target != null && context.Target != this && context.Target is IEnemy)
            {
                _lastFightTime = DateTime.Now;
                InFight = true;
            }

            var skillType = skill.GetType();
            if (!_skillExperience.ContainsKey(skillType))
                _skillExperience.Add(skillType, 1);
            _skillExperience[skillType] += Characteristics.SkillExpirienceIncrease.Value;
        }

        public void Update()
        {
            if (IsDied)
                return;

            _autoHealthIncrease.Do(() =>
            {
                ChangeHP(Characteristics.HPRecoveryRatio.Value * MaxHP, this, null);
            });

            if (DateTime.Now - _lastFightTime > FightExitTime)
                InFight = false;

            if (_buffs.Any())
                foreach (var buff in _buffs.Where(b => DateTime.Now > b.EndTime).ToArray())
                    _buffs.Remove(buff);

            Characteristics.Reset();

            ModifyCharacteristics(_usingSkillCorrector);
            ModifyCharacteristics(CheatModifier);
            ModifyCharacteristics(_defenceCorrector);
            foreach (var equipment in Equipments)
                if (equipment is IModifierCorrector corrector)
                    ModifyCharacteristics(corrector);
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

        public void ChangeHP(float hpChange, ISkilled source, ISkill skill)
        {
            if (source != null && source != this && hpChange < 0)
            {
                InFight = true;
                _lastFightTime = DateTime.Now;
            }

            if (hpChange < 0)
                hpChange /= Characteristics.DefenceRatio.Value;

            HP = Math.Min(Math.Max(0, HP + hpChange), MaxHP);
            if (source != null && skill != null && hpChange < 0)
                DagameReceived?.Invoke(new DamageInfo(source, this, skill, -hpChange));

            if (HP <= 0)
                Died?.Invoke(this);
        }

        public event Action<IHealth> Died;

        public IReadOnlyCollection<IWeapon> Weapons => _weapons;

        public IReadOnlyCollection<ISkill> Skills => _skills;

        public float GetSkillPower(ISkill skill, SkillContext context)
        {
            var r = Characteristics.PowerRatio.Value;

            if (_skillExperience.TryGetValue(skill.GetType(), out var expR))
                r *= expR;

            r *= 0.5f + HPNormalized / 2;
            if (context.Angle != null)
                r *= 2 - context.Angle.Value; // х2 при ударе в спину

            return r;
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

        public void CollectLoot([NotNull] ILoot loot)
        {
            if (loot == null) throw new ArgumentNullException(nameof(loot));

            foreach (var thing in loot.GetLoot())
                Bag.Add(thing);
        }

        public class PlayerCharacteristics
        {
            public static readonly Guid SpeedRatioId = Guid.NewGuid();
            public static readonly Guid PowerId = Guid.NewGuid();
            public static readonly Guid SkillExpirienceIncreaseId = Guid.NewGuid();
            public static readonly Guid MaxHPId = Guid.NewGuid();
            public static readonly Guid HPRecoveryRatioId = Guid.NewGuid();
            public static readonly Guid DefenceRatioId = Guid.NewGuid();

            /// <summary>
            /// Множитель к скорости передвижения
            /// </summary>
            public IModifier SpeedRatio { get; }

            /// <summary>
            /// Множитель силы ударов
            /// </summary>
            public IModifier PowerRatio { get; }

            /// <summary>
            /// Скорость накопления опыта умений
            /// </summary>
            public IModifier SkillExpirienceIncrease { get; }

            public IModifier MaxHP { get; }

            /// <summary>
            /// Скорость восстановления здоровья
            /// </summary>
            public IModifier HPRecoveryRatio { get; }

            /// <summary>
            /// Порезка входящего урона
            /// </summary>
            public IModifier DefenceRatio { get; }

            public IReadOnlyCollection<IModifier> AllModifiers { get; }

            public PlayerCharacteristics()
            {
                SpeedRatio = new Modifier(SpeedRatioId, "Скорость передвижения");
                PowerRatio = new Modifier(PowerId, "Множитель силы");
                SkillExpirienceIncrease = new Modifier(SkillExpirienceIncreaseId, "Скорость накопления опыта умений", 0.001f);
                MaxHP = new Modifier(MaxHPId, "Объём здоровья", 100);
                HPRecoveryRatio = new Modifier(HPRecoveryRatioId, "Восстановление здоровья", 0.001f);
                DefenceRatio = new Modifier(DefenceRatioId, "Защита", 1);

                AllModifiers = new[]
                {
                    SpeedRatio,
                    PowerRatio,
                    SkillExpirienceIncrease,
                    MaxHP,
                    HPRecoveryRatio,
                    DefenceRatio
                };
            }

            public void Reset()
            {
                foreach (var modifier in AllModifiers)
                    if (modifier is IWritableModifier wrModifier)
                        wrModifier.Reset();
            }
        }

        public void AddBuff([NotNull] IBuff buff)
        {
            if (buff == null) throw new ArgumentNullException(nameof(buff));
            _buffs.Add(buff);
        }

        public void PutOn([NotNull] IEquipment equipment)
        {
            if (equipment == null) throw new ArgumentNullException(nameof(equipment));

            _equipments.Add(equipment);
        }
    }

    public class SlowWhileUsingSkill : IModifierCorrector
    {
        private readonly ISkilled _skilled;

        public SlowWhileUsingSkill([NotNull] ISkilled skilled)
        {
            _skilled = skilled ?? throw new ArgumentNullException(nameof(skilled));
        }

        public void Affect([NotNull] IWritableModifier modifier)
        {
            if (modifier == null) throw new ArgumentNullException(nameof(modifier));

            if (modifier.Id == Player.PlayerCharacteristics.SpeedRatioId)
            {
                if (_skilled.Skills.OfType<ICastableSkill>().Any(sk => sk.CastingInProgress))
                    modifier.Value *= 0.5f;
            }
        }
    }

    public class Cheat : IModifierCorrector
    {
        public float Boost { get; set; }

        public Cheat()
        {
            Boost = 1;
        }

        public void Affect([NotNull] IWritableModifier modifier)
        {
            if (modifier.Id == Player.PlayerCharacteristics.PowerId)
                modifier.Value *= Boost;

            if (modifier.Id == Player.PlayerCharacteristics.MaxHPId)
                modifier.Value *= Boost;
        }
    }
}
