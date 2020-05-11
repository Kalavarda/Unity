using System;
using System.Linq;
using JetBrains.Annotations;

namespace Assets.Scripts.Utils
{
    public class AudioSourceAttribute : Attribute
    {
        private readonly string _sourceName;

        public AudioSourceAttribute([NotNull] string sourceName)
        {
            if (string.IsNullOrWhiteSpace(sourceName))
                throw new ArgumentNullException(sourceName);
            _sourceName = sourceName;
        }

        public static string GetAudioSourceName([NotNull] object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var attributes = obj.GetType().GetCustomAttributes(typeof(AudioSourceAttribute), true).OfType<AudioSourceAttribute>();
            var attribute = attributes.FirstOrDefault();
            return attribute?._sourceName;
        }
    }

    /// <summary>
    /// Название префаба для создания объекта
    /// </summary>
    public class PrefabAttribute : Attribute
    {
        private readonly string _prefabName;

        public PrefabAttribute([NotNull] string prefabName)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
                throw new ArgumentNullException(prefabName);
            this._prefabName = prefabName;
        }

        public static string GetPrefabName([NotNull] object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var attributes = obj.GetType().GetCustomAttributes(typeof(PrefabAttribute), true).OfType<PrefabAttribute>();
            var attribute = attributes.FirstOrDefault();
            return attribute?._prefabName;
        }
    }

    public class AnimationAttribute : Attribute
    {
        private readonly AnimationState _state;

        public AnimationAttribute(AnimationState state)
        {
            this._state = state;
        }

        public static AnimationState? GetAnimationState([NotNull] object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var attributes = obj.GetType().GetCustomAttributes(typeof(AnimationAttribute), true).OfType<AnimationAttribute>();
            var attribute = attributes.FirstOrDefault();
            return attribute?._state;
        }
    }
}
