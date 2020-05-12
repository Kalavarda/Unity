using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Assets.Scripts.Model
{
    public interface IEnemy
    {
        IHealth AggressionTarget { get; set; }

        IReadOnlyCollection<IBuff> Buffs { get; }

        float AggressionDistance { get; }

        void AddBuff([NotNull] IBuff buff);

        string Name { get; }
    }

    public class EnemyCharacteristics
    {
        public static readonly Guid PowerId = Guid.NewGuid();
        public static readonly Guid MaxHPId = Guid.NewGuid();
        public static readonly Guid HPRecoveryRatioId = Guid.NewGuid();

        /// <summary>
        /// Множитель силы ударов
        /// </summary>
        public IModifier PowerRatio { get; }

        public IModifier MaxHP { get; }

        /// <summary>
        /// Скорость восстановления здоровья
        /// </summary>
        public IModifier HPRecoveryRatio { get; }

        public IReadOnlyCollection<IModifier> AllModifiers { get; }

        public EnemyCharacteristics()
        {
            PowerRatio = new Modifier(PowerId, "Множитель силы");
            MaxHP = new Modifier(MaxHPId, "Объём здоровья", 100);
            HPRecoveryRatio = new Modifier(HPRecoveryRatioId, "Восстановление здоровья", 0.001f);

            AllModifiers = new[]
            {
                PowerRatio,
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

    public class EnemyCorrector : IModifierCorrector
    {
        private readonly float _hpRatio;
        private readonly float _powerRatio;

        public EnemyCorrector(float hpRatio, float powerRatio)
        {
            _hpRatio = hpRatio;
            _powerRatio = powerRatio;
        }

        public void Affect([NotNull] IWritableModifier modifier)
        {
            if (modifier.Id == EnemyCharacteristics.PowerId)
                modifier.Value *= _powerRatio;

            if (modifier.Id == EnemyCharacteristics.MaxHPId)
                modifier.Value *= _hpRatio;
        }
    }
}
