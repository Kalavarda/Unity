namespace Assets.Scripts.Model
{
    public class FlowerPrototype : IThingPrototype
    {
        public static FlowerPrototype Instance { get; } = new FlowerPrototype();

        public string Name => "Цветок";

        public int BagMaxStackCount => 1000;
        
        public IThing CreateInstance()
        {
            return new Stack(Instance);
        }
    }

    public class MeatPrototype : IThingPrototype
    {
        public static MeatPrototype Instance { get; } = new MeatPrototype();

        public string Name => "Мясо";

        public int BagMaxStackCount => 10;

        public IThing CreateInstance()
        {
            return new Stack(Instance);
        }
    }
}
