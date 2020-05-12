using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Assets.Scripts.Model.Things
{
    public class Chest: IThing, ILoot
    {
        private readonly IReadOnlyCollection<IThing> _loot;

        public IThingPrototype Prototype => throw new NotImplementedException();

        public Chest([NotNull] IReadOnlyCollection<IThing> loot)
        {
            _loot = loot ?? throw new ArgumentNullException(nameof(loot));
        }
        
        public IReadOnlyCollection<IThing> GetLoot()
        {
            return _loot;
        }
    }
}
