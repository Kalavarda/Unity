using System.Collections.Generic;

namespace Assets.Scripts.Model
{
    public interface IArmed
    {
        IReadOnlyCollection<IWeapon> Weapons { get; }
    }
}
