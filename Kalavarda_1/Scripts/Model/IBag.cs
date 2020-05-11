using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Assets.Scripts.Model
{
    public interface IBag
    {
        IReadOnlyCollection<IBagCell> Cells { get; }

        void Add([NotNull] IThing thing);

        /// <summary>
        /// Достать из сумки указанное кол-во предметов
        /// </summary>
        IReadOnlyCollection<IThing> Pull([NotNull] IReadOnlyCollection<IStack> stacks);

        /// <summary>
        /// Достать из сумки указанное кол-во предметов
        /// </summary>
        IReadOnlyCollection<IThing> Pull([NotNull]IStack stack);

        /// <summary>
        /// Достать предмет из сумки
        /// </summary>
        IThing Pull([NotNull]IThing thing);

        event Action<IBag, IBagCell> OnCellAdded;

        event Action<IBag, IBagCell> OnCellRemoved;
    }

    public interface IBagCell
    {
        IThing Item { get; }

        string Name { get; }
        
        void Add(IStack stack);
        
        void Remove(IStack stack);

        event Action<IBagCell> OnStackCountChange;
    }
}
