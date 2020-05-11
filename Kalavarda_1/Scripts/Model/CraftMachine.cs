using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Assets.Scripts.Model.Enemies;
using Assets.Scripts.Model.Things;
using JetBrains.Annotations;

namespace Assets.Scripts.Model
{
    public class CraftMachine : ICraftMachine
    {
        private readonly IBag _bag;

        public IReadOnlyCollection<Recipe> Recipes { get; }

        private readonly IChanceCalculator _chanceCalculator = new Chance();

        public CraftMachine([NotNull] IBag bag, [NotNull] IRecipeSource recipeSource)
        {
            if (recipeSource == null) throw new ArgumentNullException(nameof(recipeSource));
            _bag = bag ?? throw new ArgumentNullException(nameof(bag));

            Recipes = recipeSource.GetRecipes();
        }

        public CraftResult Craft(Recipe recipe, float chance)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));

            var sourceItems = _bag.Pull(recipe.SourceItems).ToList();

            var result = new Collection<IThing>();
            var crackedSourceItems = new Collection<IThing>();

            foreach (var stack in recipe.SourceItems)
            {
                //for (var i = 0; i < stack.Count; i++)
                {
                    var sourceItem = sourceItems.FirstOrDefault(it => it.Prototype == stack.Prototype);
                    if (sourceItem == null)
                        throw new Exception("Недостаточно предметов " + stack.Prototype.Name);
                    sourceItems.Remove(sourceItem);
                    crackedSourceItems.Add(sourceItem);
                }
            }
            if (_chanceCalculator.Try(chance))
                Create(recipe, result);

            // результат крафта кладём в сумку
            foreach (var bagItem in result)
                _bag.Add(bagItem);

            // если вдруг не израсходовались, положить обратно
            foreach (var item in sourceItems)
                _bag.Add(item);

            return new CraftResult(result, crackedSourceItems);
        }

        private static void Create(Recipe recipe, ICollection<IThing> result)
        {
            for (var i = 0; i < recipe.Result.Count; i++)
            {
                var instance = recipe.Result.Prototype.CreateInstance();
                result.Add(instance);
            }
        }
    }

    public class Recipes1 : IRecipeSource
    {
        public IReadOnlyCollection<Recipe> GetRecipes()
        {
            return new[]
            {
                new Recipe(new Stack(AxePrototype.Instance), new []
                {
                    new Stack(StickPrototype.Instance),
                    new Stack(StonePrototype.Instance),
                }),
                new Recipe(new Stack(ScalpNecklacePrototype.Instance), new []
                {
                    new Stack(ScalpPrototype.Instance, 10),
                    new Stack(PantsPrototype.Instance, 10),
                }),
                new Recipe(new Stack(ToothNecklacePrototype.Instance), new []
                {
                    new Stack(UnderwearPrototype.Instance, 10),
                    new Stack(HumanToothPrototype.Instance, 100),
                }),
            };
        }
    }
}
