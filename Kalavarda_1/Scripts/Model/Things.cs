using Assets.Scripts.Utils;

namespace Assets.Scripts.Model
{
    [Prefab("Stone")]
    public class Stone : IThing, ISpawned
    {
        public IThingPrototype Prototype => StonePrototype.Instance;
    }

    public class StonePrototype : IThingPrototype
    {
        public static StonePrototype Instance { get; } = new StonePrototype();

        public string Name => "Камень";

        public int BagMaxStackCount => 1;

        public IThing CreateInstance()
        {
            return new Stone();
        }
    }
}
