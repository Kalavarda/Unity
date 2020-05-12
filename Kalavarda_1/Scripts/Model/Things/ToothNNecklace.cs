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
/*
    public class ToothNecklaceRecipePrototype : IThingPrototype
    {
        public static ToothNecklaceRecipePrototype Instance { get; } = new ToothNecklaceRecipePrototype();

        public string Name => "Рецепт " + ToothNecklacePrototype.Instance.Name;

        public int BagMaxStackCount => 1;

        public IThing CreateInstance()
        {
            return new Recipe(
                new Stack(ToothNecklacePrototype.Instance), new[]
                {
                    new Stack(HumanToothPrototype.Instance, 100),
                    new Stack(UnderwearPrototype.Instance, 10),
                });
        }
    }
*/
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
/*
    public class ScalpNecklaceRecipePrototype : IThingPrototype
    {
        public static ScalpNecklaceRecipePrototype Instance { get; } = new ScalpNecklaceRecipePrototype();

        public string Name => "Рецепт " + ScalpNecklacePrototype.Instance.Name;

        public int BagMaxStackCount => 1;

        public IThing CreateInstance()
        {
            return new Recipe(new Stack(ScalpNecklacePrototype.Instance),
            new[]
            {
                new Stack(ScalpPrototype.Instance, 10),
                new Stack(PantsPrototype.Instance, 10),
            });
        }
    }
*/
}
