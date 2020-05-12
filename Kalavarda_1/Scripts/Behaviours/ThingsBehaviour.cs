using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model;
using Assets.Scripts.Model.Enemies;
using Assets.Scripts.Model.Things;
using Assets.Scripts.Utils;
using JetBrains.Annotations;
using UnityEngine;

public class ThingsBehaviour : MonoBehaviour
{
    public static ThingsBehaviour Instance { get; private set; }

    private readonly Dictionary<IThing, GameObject> _things = new Dictionary<IThing, GameObject>();

    public IReadOnlyDictionary<IThing, GameObject> Things => _things;

    void Start()
    {
        Instance = this;

        var chest01 = new Chest(new[]
        {
            new Recipe(
                new Stack(ToothNecklacePrototype.Instance), new[]
                {
                    new Stack(HumanToothPrototype.Instance, 50),
                    new Stack(UnderwearPrototype.Instance, 5),
                })
        });
        _things.Add(chest01, GameObject.Find("Chest_01"));

        var chest02 = new Chest(new[]
        {
            new Recipe(
                new Stack(ScalpNecklacePrototype.Instance), new[]
                {
                    new Stack(ScalpPrototype.Instance, 5),
                    new Stack(PantsPrototype.Instance, 5),
                })
        });
        _things.Add(chest02, GameObject.Find("Chest_02"));
    }

    public IThing Search([NotNull] GameObject gameObject)
    {
        if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));

        foreach (var pair in _things.Where(p => p.Value.activeSelf))
            if (pair.Value == gameObject)
                return pair.Key;

        return null;
    }

    /// <summary>
    /// Найти ближайший к указанному объекту
    /// </summary>
    public IThing SearchNearly([NotNull] GameObject gameObject)
    {
        if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));

        var near = _things.Values.Where(obj => obj.activeSelf).OrderBy(p => Utils.Distance(gameObject, p)).FirstOrDefault();
        if (near == null)
            return null;

        return Search(near);
    }
}
