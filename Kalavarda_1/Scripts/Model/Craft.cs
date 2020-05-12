using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Assets.Scripts.Model
{
    public interface ICraftMachine
    {
        /// <summary>
        /// Скрафтить
        /// </summary>
        CraftResult Craft([NotNull] Recipe recipe, float chance);
    }

    public class CraftResult
    {
        public IReadOnlyCollection<IThing> Result { get; }

        public IReadOnlyCollection<IThing> CrackedSourceItems { get; }

        public CraftResult([NotNull] IReadOnlyCollection<IThing> result, IReadOnlyCollection<IThing> crackedSourceItems)
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
            if (crackedSourceItems != null) CrackedSourceItems = crackedSourceItems;
        }
    }

    public class Recipe: IThing
    {
        public IStack Result { get; }

        public IReadOnlyCollection<IStack> SourceItems { get; }

        public Recipe([NotNull] IStack result, [NotNull] IReadOnlyCollection<IStack> sourceItems)
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
            SourceItems = sourceItems ?? throw new ArgumentNullException(nameof(sourceItems));
            Prototype = new RecipePrototype(result.Prototype);
        }

        public IThingPrototype Prototype { get; }
    }

    public class RecipePrototype : IThingPrototype
    {
        private readonly IThingPrototype _targetPrototype;

        public string Name => "Рецепт " + _targetPrototype.Name;

        public int BagMaxStackCount => 1;

        public IThing CreateInstance()
        {
            throw new NotImplementedException();
        }

        public RecipePrototype([NotNull] IThingPrototype targetPrototype)
        {
            _targetPrototype = targetPrototype ?? throw new ArgumentNullException(nameof(targetPrototype));
        }
    }


    public interface IRecipeCollection
    {
        /// <summary>
        /// Возвращает доступные рецепты
        /// </summary>
        IReadOnlyCollection<Recipe> GetRecipes();

        void Add([NotNull] Recipe recipe);

        event Action<IRecipeCollection> Changed;
    }
}
