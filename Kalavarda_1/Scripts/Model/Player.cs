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
        private static readonly TimeSpan FightExitTime = TimeSpan.FromSeconds(20);
        private readonly TimeIntervalLimiter _autoHealthIncrease = new TimeIntervalLimiter(TimeSpan.FromSeconds(1));

        private DateTime _lastFightTime = DateTime.MinValue;

        private readonly List<IWeapon> _weapons = new List<IWeapon>();
        private readonly List<ISkill> _skills = new List<ISkill>();
        private bool _inFight;

        private readonly List<IEquipment> _equipments = new List<IEquipment>();
        private readonly List<IBuff> _buffs = new List<IBuff>();

        public IReadOnlyCollection<IEquipment> Equipments => _equipments;

        public IReadOnlyCollection<IBuff> Buffs => _buffs;

        public PlayerCharacteristics Characteristics { get; }

        private readonly Dictionary<Type, float> _skillExperience = new Dictionary<Type, float>();
        private readonly SlowWhileUsingSkill _usingSkillCorrector;
        internal readonly Cheat CheatModifier = new Cheat();

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
            _skills.Add(new SimplePunch(fist));

            _skills.Add(new Throwing());
/*
            Bag.Add(new Stack(ScalpPrototype.Instance, 15));
            Bag.Add(new Stack(UnderwearPrototype.Instance, 15));
            Bag.Add(new Stack(HumanToothPrototype.Instance, 150));
            Bag.Add(new Stack(PantsPrototype.Instance, 15));
            Bag.Add(new Stack(MeatPrototype.Instance, 5));
*/
        }

        public void Update()
        {
            _autoHealthIncrease.Do(() =>
            {
                ChangeHP(Characteristics.HPRecoveryRatio.Value * MaxHP, this, null);
            });

            if (DateTime.Now - _lastFightTime > FightExitTime)
                InFight = false;

            foreach (var buff in _buffs.Where(b => DateTime.Now > b.EndTime).ToArray())
                _buffs.Remove(buff);

            Characteristics.Reset();
            foreach (var modifier in Characteristics.AllModifiers)
                if (modifier is IWritableModifier wrModifier)
                {
                    _usingSkillCorrector.Affect(wrModifier);

                    foreach (var equipment in Equipments)
                        if (equipment is IModifierCorrector corrector)
                            corrector.Affect(wrModifier);

                    CheatModifier.Affect(wrModifier);

                    foreach (var buff in _buffs)
                        if (buff is IModifierCorrector corrector)
                            corrector.Affect(wrModifier);
                }
        }

        public void ChangeHP(float hpChange, IHealth source, ISkill skill)
        {
            if (source != null && source != this && hpChange < 0)
            {
                InFight = true;
                _lastFightTime = DateTime.Now;
            }

            HP = Math.Min(Math.Max(0, HP + hpChange), MaxHP);
            if (source != null && skill != null && hpChange < 0)
                DagameReceived?.Invoke(new DamageInfo(source, this, skill, -hpChange));

            if (HP <= 0)
                Died?.Invoke(this);
        }

        public event Action<IHealth> Died;

        public IReadOnlyCollection<IWeapon> Weapons => _weapons;

        public IReadOnlyCollection<ISkill> Skills => _skills;

        public float GetSkillPower(ISkill skill)
        {
            var r = Characteristics.PowerRatio.Value;

            if (_skillExperience.TryGetValue(skill.GetType(), out var expR))
                r *= expR;

            r *= 0.5f + HPNormalized / 2;
            
            return r;
        }

        public void Use([NotNull] ISkill skill, IHealth target, float distance, Action onStartUse)
        {
            if (skill == null) throw new ArgumentNullException(nameof(skill));

            if (!skill.ReadyToUse(target, distance))
                return;

            skill.Use(target, this, distance, () =>
            {
                if (target != null && target != this && target is IEnemy)
                {
                    _lastFightTime = DateTime.Now;
                    InFight = true;
                }

                var skillType = skill.GetType();
                if (!_skillExperience.ContainsKey(skillType))
                    _skillExperience.Add(skillType, 1);
                _skillExperience[skillType] += Characteristics.SkillExpirienceIncrease.Value;

                onStartUse();
            });
        }

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

            public IReadOnlyCollection<IModifier> AllModifiers { get; }

            public PlayerCharacteristics()
            {
                SpeedRatio = new Modifier(SpeedRatioId, "Скорость передвижения");
                PowerRatio = new Modifier(PowerId, "Множитель силы");
                SkillExpirienceIncrease = new Modifier(SkillExpirienceIncreaseId, "Скорость накопления опыта умений", 0.001f);
                MaxHP = new Modifier(MaxHPId, "Объём здоровья", 100);
                HPRecoveryRatio = new Modifier(HPRecoveryRatioId, "Восстановление здоровья", 0.001f);

                AllModifiers = new[]
                {
                    SpeedRatio,
                    PowerRatio,
                    SkillExpirienceIncrease,
                    MaxHP,
                    HPRecoveryRatio
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
                if (_skilled.Skills.OfType<ICastableSkill>().Any(sk => sk.IsCasting))
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
