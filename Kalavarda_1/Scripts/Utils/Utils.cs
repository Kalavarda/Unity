using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class Utils
    {
        public static float Distance([NotNull] GameObject obj1, [NotNull] GameObject obj2)
        {
            if (obj1 == null)
                throw new ArgumentNullException(nameof(obj1));
            if (obj2 == null)
                throw new ArgumentNullException(nameof(obj2));

            var mr1 = obj1.GetComponent<MeshRenderer>();
            var c1 = mr1 != null ? mr1.bounds.center : obj1.transform.position;

            var mr2 = obj2.GetComponent<MeshRenderer>();
            var c2 = mr2 != null ? mr2.bounds.center : obj2.transform.position;

            var distance = Vector3.Distance(c1, c2);

            return distance;
        }
    }
}
