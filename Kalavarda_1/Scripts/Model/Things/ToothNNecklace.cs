using System;
using JetBrains.Annotations;

namespace Assets.Scripts.Model.Things
{
    public class ToothNNecklace : IThing, IEquipment, IModifierCorrector
    {
        public IThingPrototype Prototype => ToothNecklacePrototype.Instance;

        public void Affect([NotNull] IWritableModifier modifier)
        {
            if (modifier == null) throw new ArgumentNullException(nameof(modifier));

            if (modifier.Id == Player.PlayerCharacteristics.PowerId)
                modifier.Value *= 1.5f;
        }
    }

    public class ToothNecklacePrototype : IThingPrototype
    {
        public static ToothNecklacePrototype Instance { get; } = new ToothNecklacePrototype();

        public string Name => "Ожерелье из зубов";

        public int BagMaxStackCount => 1;

        public IThing CreateInstance()
        {
            return new ToothNNecklace();
        }
    }

    public class ScalpNecklace : IThing, IEquipment, IModifierCorrector
    {
        public IThingPrototype Prototype => ScalpNecklacePrototype.Instance;
        
        public void Affect([NotNull] IWritableModifier modifier)
        {
            if (modifier == null) throw new ArgumentNullException(nameof(modifier));

            if (modifier.Id == Player.PlayerCharacteristics.PowerId)
                modifier.Value *= 1.5f;
        }
    }

    public class ScalpNecklacePrototype : IThingPrototype
    {
        public static ScalpNecklacePrototype Instance { get; } = new ScalpNecklacePrototype();

        public string Name => "Ожерелье из скальпов";

        public int BagMaxStackCount => 1;

        public IThing CreateInstance()
        {
            return new ScalpNecklace();
        }
    }
}
