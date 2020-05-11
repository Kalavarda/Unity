using System;
using JetBrains.Annotations;

namespace Assets.Scripts.Model
{
    /// <summary>
    /// Некий предмет
    /// </summary>
    public interface IThing
    {
        [NotNull]
        IThingPrototype Prototype { get; }
    }

    /// <summary>
    /// Куча однотипных предметов
    /// </summary>
    public interface IStack
    {
        [NotNull]
        IThingPrototype Prototype { get; }

        int Count { get; set; }

        event Action<IStack> CountChanged;
    }

    /// <summary>
    /// Описание типа предметов
    /// </summary>
    public interface IThingPrototype
    {
        string Name { get; }

        /// <summary>
        /// Сколько может стаковаться в одной ячейке
        /// </summary>
        int BagMaxStackCount { get; }

        /// <summary>
        /// Создаёт экземпляр объекта, прототипом которого является
        /// </summary>
        IThing CreateInstance();
    }

    public class Stack: IStack, IThing
    {
        private int count;

        [NotNull]
        public IThingPrototype Prototype { get; }

        public int Count
        {
            get => count;
            set
            {
                if (count == value)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                count = value;
                CountChanged?.Invoke(this);
            }
        }

        public event Action<IStack> CountChanged;

        public Stack([NotNull] IThingPrototype prototype, int count = 1)
        {
            Prototype = prototype ?? throw new ArgumentNullException(nameof(prototype));

            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            Count = count;
        }

        public string Name => $"{Prototype.Name} ({Count})";

        public override string ToString()
        {
            return Name;
        }
    }

    public static class ThingCreator
    {
        public static IThing C_reate([NotNull] IThingPrototype prototype)
        {
            if (prototype == null) throw new ArgumentNullException(nameof(prototype));

            throw new NotImplementedException();
        }
    }
}
