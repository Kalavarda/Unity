using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Assets.Scripts.Model;
using Assets.Scripts.Model.Enemies;
using Assets.Scripts.Utils;
using UnityEngine;
using AnimationState = Assets.Scripts.AnimationState;

public class SpawnerBehaviour : MonoBehaviour
{
    [Tooltip("Какой объект клонировать")]
    public GameObject CloneSource;
    
    [Tooltip("в секундах")]
    public float Period = 60;

    [Tooltip("Радиус, в котором будут спаунится клоны")]
    public float Radius = 10;

    [Tooltip("Группировать в иерархии объектов внутрь спаунера")]
    public bool SetParent = true;

    [Tooltip("Высота над спаунером при появлении")]
    public float HeightAtSpawner = 1;

    public bool RandomRotation = false;

    [Tooltip("Скрывать префаб")]
    public bool SetPrefabInactive = true;

    [Tooltip("Не спаунить, если уже есть столько объектов")]
    public int Limit = 1;

    [Tooltip("Уничтожать объект в случае его смерти")]
    public bool DestroyIfDeath;

    private static readonly System.Random Random = new System.Random();
    private TimeIntervalLimiter _limiter;
    private readonly ICollection<ISpawned> _spawned = new Collection<ISpawned>();

    private static readonly Dictionary<GameObject, ISpawned> _allSpawned = new Dictionary<GameObject, ISpawned>();
    private readonly Player _player = Player.Instance;

    public static IReadOnlyDictionary<GameObject, ISpawned> AllSpawned => _allSpawned;

    public static IReadOnlyDictionary<GameObject, IHealth> SpawnedHealth
    {
        get
        {
            return AllSpawned.Where(p => p.Value is IHealth).ToDictionary(p => p.Key, p => p.Value as IHealth);
        }
    }

    void Start()
    {
        _limiter = new TimeIntervalLimiter(TimeSpan.FromSeconds(Period));
    }

    void Update()
    {
        _limiter.Do(() =>
        {
            var count = 0;
            foreach (var sp in _spawned)
                if (AllSpawned.Any(p => p.Value == sp))
                    count++;
            if (count >= Limit)
                return;

            if (SetPrefabInactive)
                CloneSource.SetActive(true);
            var clone = Instantiate(CloneSource, transform.position, transform.rotation);
            if (SetPrefabInactive)
                CloneSource.SetActive(false);

            if (SetParent)
                clone.transform.SetParent(transform, false);
            
            clone.transform.rotation = RandomRotation
                ? new Quaternion((float)Random.NextDouble(), (float)Random.NextDouble(), (float)Random.NextDouble(), (float)Random.NextDouble())
                : CloneSource.transform.rotation;

            Bounds bounds = gameObject.GetComponent<MeshRenderer>().bounds;
            var r = GetRandomVector(bounds.extents) * Radius;
            var pos = transform.position;
            pos = new Vector3(pos.x + r.x, pos.y + HeightAtSpawner, pos.z + r.z);
            clone.transform.SetPositionAndRotation(pos, clone.transform.rotation);

            var spawned = CreateISpawned(CloneSource);
            AddSpawned(clone, spawned);
            _spawned.Add(spawned);

            if (spawned is IHealth health)
                health.Died += Health_Died;
        });
    }

    private void Health_Died(IHealth dead)
    {
        dead.Died -= Health_Died;

        var deadGameObject = AllSpawned.First(p => p.Value == dead).Key;
        if (DestroyIfDeath)
        {
            RemoveSpawned(deadGameObject);
            Destroy(deadGameObject);
        }
        else
        {
            var animMaganer = AnimationManagerBase.CreateOrGet(deadGameObject);
            animMaganer.SetState(AnimationState.Die);
        }

        if (dead is ILoot loot)
            _player.CollectLoot(loot);
    }

    private static ISpawned CreateISpawned(GameObject prefab)
    {
        if (prefab.name == "Enemy_01") // male
            return new HumanEnemy(1, 1.5f, new IThing[]
            {
                new Stack(ScalpPrototype.Instance),
                new Stack(PantsPrototype.Instance),
                new Stack(MeatPrototype.Instance, 4)
            });

        if (prefab.name == "Enemy_02") // female
            return new HumanEnemy(1, 0.666f, new IThing[]
            {
                new Stack(HumanToothPrototype.Instance, 10),
                new Stack(UnderwearPrototype.Instance),
                new Stack(MeatPrototype.Instance, 3)
            }, true);

        throw new NotImplementedException();
    }

    // TODO: возможно, вынести в Utils
    internal static Vector3 GetRandomVector(Vector3 size)
    {
        var dx = (float)Random.NextDouble() * Math.Abs(size.x);
        var dy = (float)Random.NextDouble() * Math.Abs(size.y);
        var dz = (float)Random.NextDouble() * Math.Abs(size.z);
        if (Random.Next(2) == 0)
            dx = -dx;
        if (Random.Next(2) == 0)
            dy = -dy;
        if (Random.Next(2) == 0)
            dz = -dz;
        return new Vector3(dx, dy, dz);
    }

    public static void AddSpawned(GameObject gameObject, ISpawned spawned)
    {
        _allSpawned.Add(gameObject, spawned);
    }

    public static void RemoveSpawned(GameObject gameObject)
    {
        _allSpawned.Remove(gameObject);
    }
}
