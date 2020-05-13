using System;
using JetBrains.Annotations;

namespace Assets.Scripts.Model
{
    public interface IModifier
    {
        float Value { get; }

        string Name { get; }
    }

    public interface IWritableModifier
    {
        Guid Id { get; }

        float Value { get; set; }

        /// <summary>
        /// Сбросить значение к начальному
        /// </summary>
        void Reset();
    }

    public interface IModifierCorrector
    {
        /// <summary>
        /// Скорректировать значение
        /// </summary>
        void Affect(IWritableModifier modifier);
    }

    public class Modifier : IModifier, IWritableModifier
    {
        private readonly float _startValue;

        public Guid Id { get; }

        public float Value { get; set; }

        public string Name { get; }

        public void Reset()
        {
            Value = _startValue;
        }

        public Modifier(Guid id, string name, float startValue = 1)
        {
            Id = id;
            Name = name;
            _startValue = startValue;
            Reset();
        }

        public override string ToString()
        {
            return $"{Name} = {Value}";
        }
    }

    public class DefenceCorrector : IModifierCorrector
    {
        private readonly IHealth _health;

        public DefenceCorrector([NotNull] IHealth health)
        {
            _health = health ?? throw new ArgumentNullException(nameof(health));
        }

        public void Affect([NotNull] IWritableModifier modifier)
        {
            if (modifier.Id == EnemyCharacteristics.DefenceRatioId || modifier.Id == Player.PlayerCharacteristics.DefenceRatioId)
                modifier.Value *= 0.5f + 0.5f * _health.HPNormalized;
        }
    }
}
