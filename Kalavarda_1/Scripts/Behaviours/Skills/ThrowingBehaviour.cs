using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Assets.Scripts.Model;
using Assets.Scripts.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.Behaviours.Skills
{
    public class ThrowingBehaviour: MonoBehaviour
    {
        private const float ThrowPower = 2500f;

        private static Timer _timer;

        private static readonly List<Tuple<IThrowingSkill, GameObject, DateTime>> _throwedObjects = new List<Tuple<IThrowingSkill, GameObject, DateTime>>();
        private static readonly TimeSpan MaxThrowedLifetime = TimeSpan.FromSeconds(3);

        public static void AddThrowing([NotNull] IThrowingSkill skill, [NotNull] GameObject throwedObject)
        {
            if (skill == null) throw new ArgumentNullException(nameof(skill));
            if (throwedObject == null) throw new ArgumentNullException(nameof(throwedObject));

            if (_timer == null) // это чтобы удалять объекты, ни с чем не столкнувшиеся
                _timer = new Timer(OnTimer, null, MaxThrowedLifetime, TimeSpan.FromSeconds(1));

            _throwedObjects.Add(new Tuple<IThrowingSkill, GameObject, DateTime>(skill, throwedObject, DateTime.Now));
        }

        private static void OnTimer(object obj)
        {
            var toRemove = _throwedObjects.Where(t => (DateTime.Now - t.Item3) > MaxThrowedLifetime).ToArray();
            foreach (var tuple in toRemove)
            {
                tuple.Item1.OnOnCollisionEnter(null, 0);
                Destroy(tuple.Item2);
                _throwedObjects.Remove(tuple);
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            var tuple = _throwedObjects.First(t => t.Item2 == gameObject);
            _throwedObjects.Remove(tuple);

            IHealth health = null;
            var velocity = 0f;
            if (SpawnerBehaviour.AllSpawned.TryGetValue(collision.gameObject, out var spawned))
            {
                if (spawned is IHealth h)
                {
                    health = h;
                    velocity = collision.relativeVelocity.magnitude;
                }
                tuple.Item1.OnOnCollisionEnter(health, velocity);
            }
            else
                tuple.Item1.OnOnCollisionEnter(null, velocity);

            Destroy(tuple.Item2);
        }

        public static void Throw(IThrowingSkill throwing, float pressDurationNormalized)
        {
            var startTransform = PlayerBehaviour.Instance.PlayerGameObject.transform;

            var original = GameObject.Find(PrefabAttribute.GetPrefabName(throwing.Thing));
            var clone = Instantiate(original, startTransform.position, startTransform.rotation);

            var startPosition = new Vector3(startTransform.position.x, startTransform.position.y, startTransform.position.z);
            startPosition += startTransform.up * 2.0f;

            clone.transform.SetPositionAndRotation(startPosition, startTransform.rotation);
            AddThrowing(throwing, clone);

            var rigidbody = clone.GetComponent<Rigidbody>();
            var power = (0.1f + 0.9f * pressDurationNormalized) * ThrowPower;
            var angle = 0.25f * pressDurationNormalized;
            rigidbody.AddForce(Vector3.Lerp(startTransform.forward, startTransform.up, angle) * power, ForceMode.Force);
        }
    }
}
