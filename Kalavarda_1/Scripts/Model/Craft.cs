using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Assets.Scripts.Model.Enemies;
using Assets.Scripts.Model.Things;
using JetBrains.Annotations;

namespace Assets.Scripts.Model
{
    public interface ICraftMachine
    {
        /// <summary>
        /// Доступные рецепты
        /// </summary>
        IReadOnlyCollection<Recipe> Recipes { get; }

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

    public class Recipe
    {
        public IStack Result { get; }

        public IReadOnlyCollection<IStack> SourceItems { get; }

        public Recipe([NotNull] IStack result, [NotNull] IReadOnlyCollection<IStack> sourceItems)
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
            SourceItems = sourceItems ?? throw new ArgumentNullException(nameof(sourceItems));
        }
    }

    public interface IRecipeSource
    {
        /// <summary>
        /// Возвращает доступные рецепты
        /// </summary>
        IReadOnlyCollection<Recipe> GetRecipes();
    }
}
