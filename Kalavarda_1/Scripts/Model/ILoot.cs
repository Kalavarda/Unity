using System.Collections.Generic;

namespace Assets.Scripts.Model
{
    public interface ILoot
    {
        IReadOnlyCollection<IThing> GetLoot();
    }
}
