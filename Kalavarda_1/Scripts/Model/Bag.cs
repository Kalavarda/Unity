using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Assets.Scripts.Model;
using JetBrains.Annotations;

namespace Assets.Scripts
{
    public class Bag : IBag
    {
        private readonly ICollection<IBagCell> _cells = new List<IBagCell>();

        public IReadOnlyCollection<IBagCell> Cells => _cells.ToArray();

        public void Add(IThing thing)
        {
            if (thing == null) throw new ArgumentNullException(nameof(thing));

            if (thing is IStack stack)
            {
                // ячейки с такими же предметами
                var cells = _cells.Where(c => c.Item is IStack s && s.Prototype == stack.Prototype);

                // TODO: найти ячейку, в которой стак ещё не заполнен полностью (учесть лимит стака)

                var cell = cells.FirstOrDefault();
                if (cell == null)
                {
                    cell = new BagCell(thing);
                    _cells.Add(cell);
                    cell.OnStackCountChange += CellOnStackCountChange;
                    OnCellAdded?.Invoke(this, cell);
                }
                else
                    ((IStack)cell.Item).Count += stack.Count;
            }
            else
            {
                var cell = new BagCell(thing);
                _cells.Add(cell);
                cell.OnStackCountChange += CellOnStackCountChange;
                OnCellAdded?.Invoke(this, cell);
            }
        }

        private void CellOnStackCountChange(IBagCell cell)
        {
            if (cell.Item is IStack stack)
                if (stack.Count == 0)
                {
                    _cells.Remove(cell);
                    cell.OnStackCountChange -= CellOnStackCountChange;
                    OnCellRemoved?.Invoke(this, cell);
                }
        }

        public IReadOnlyCollection<IThing> Pull(IReadOnlyCollection<IStack> stacks)
        {
            if (stacks == null) throw new ArgumentNullException(nameof(stacks));

            var result = new List<IThing>();
            try
            {
                foreach (var stack in stacks)
                    result.AddRange(Pull(stack));
            }
            catch
            {
                foreach (var item in result)
                    Add(item);
                throw;
            }

            return result;
        }

        public IReadOnlyCollection<IThing> Pull(IStack stack)
        {
            if (stack == null) throw new ArgumentNullException(nameof(stack));

            if (stack.Count == 0)
                return new IThing[0];

            var stackCells = _cells.Where(c => c.Item is IStack s && s.Prototype == stack.Prototype).ToArray();
            if (stackCells.Any())
            {
                if (stackCells.Sum(c => ((IStack)c.Item).Count) < stack.Count)
                    throw new Exception("Недостаточно предметов " + stack.Prototype.Name);

                // TODO: тут надо бы с учётом того что предметы могут быть размазаны по разным стаков
                stackCells.First().Remove(stack);

                return new IThing[] { new Stack(stack.Prototype, stack.Count) };
            }

            var cells = _cells.Where(c => c.Item.Prototype == stack.Prototype).Take(stack.Count).ToArray();
            if (cells.Length < stack.Count)
                throw new Exception("Недостаточно предметов " + stack.Prototype.Name);

            var result = new Collection<IThing>();

            foreach (var cell in cells)
            {
                result.Add(cell.Item);
                cell.OnStackCountChange -= CellOnStackCountChange;
                _cells.Remove(cell);
                OnCellRemoved?.Invoke(this, cell);
            }

            return result;
        }

        public IThing Pull(IThing thing)
        {
            if (thing == null) throw new ArgumentNullException(nameof(thing));

            var cell = _cells.First(c => c.Item == thing);
            cell.OnStackCountChange -= CellOnStackCountChange;
            _cells.Remove(cell);
            OnCellRemoved?.Invoke(this, cell);

            return thing;
        }

        public event Action<IBag, IBagCell> OnCellAdded;
        public event Action<IBag, IBagCell> OnCellRemoved;
    }

    public class BagCell : IBagCell, IDisposable
    {
        public IThing Item { get; private set; }

        public string Name
        {
            get
            {
                if (Item is IStack stack)
                    return $"{stack.Prototype.Name} ({stack.Count})";

                return Item.Prototype.Name;
            }
        }

        public void Add([NotNull] IStack stack)
        {
            if (stack == null) throw new ArgumentNullException(nameof(stack));

            if (Item is IStack s)
            {
                if (s.Prototype != stack.Prototype)
                    throw new Exception("Bag error");

                s.Count += stack.Count;
            }
            else
                throw new Exception("Bag error");
        }

        public void Remove([NotNull] IStack stack)
        {
            if (stack == null) throw new ArgumentNullException(nameof(stack));

            if (Item is IStack s)
            {
                if (s.Prototype != stack.Prototype)
                    throw new Exception("Bag error");

                if (s.Count < stack.Count)
                    throw new Exception("Bag error");

                s.Count -= stack.Count;
            }
            else
                throw new Exception("Bag error");
        }

        public event Action<IBagCell> OnStackCountChange;

        public BagCell([NotNull] IThing thing)
        {
            Item = thing ?? throw new ArgumentNullException(nameof(thing));

            if (thing is IStack stack)
                stack.CountChanged += Stack_CountChanged;
        }

        private void Stack_CountChanged(IStack stack)
        {
            OnStackCountChange?.Invoke(this);
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? base.ToString() : Name;
        }

        public void Dispose()
        {
            if (Item is IStack stack)
                stack.CountChanged -= Stack_CountChanged;
            Item = null;
        }
    }
}
