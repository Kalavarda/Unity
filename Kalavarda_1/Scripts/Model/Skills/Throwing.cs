using System;
using Assets.Scripts.Utils;

namespace Assets.Scripts.Model.Skills
{
    [AudioSource("Throwing")]
    public class Throwing : ISkill, IThrowingSkill, ICastableSkill
    {
        private DateTime _lastUseTime = DateTime.MinValue;
        private IHealth _target, _source;
        private readonly Stone _thing = new Stone();
        private DateTime? _beginCastTime;
        private DateTime? _endCastTime;

        public string Name => "Бросок";

        public string Description => "Бросает " + Thing.Prototype.Name;

        public TimeSpan Interval => TimeSpan.FromSeconds(10);

        public TimeSpan Cooldown
        {
            get
            {
                if (_lastUseTime + Interval < DateTime.Now)
                    return TimeSpan.Zero;

                return _lastUseTime + Interval - DateTime.Now;
            }
        }

        public bool ReadyToUse => _target == null && Cooldown == TimeSpan.Zero;

        public float CooldownNormalized => (float)Cooldown.TotalSeconds / (float)Interval.TotalSeconds;

        public void Use(IHealth target, IHealth source, float distance, Action onStartUse)
        {
            if (Cooldown > TimeSpan.Zero)
                return;

            if (_target != null) // значит, ещё предыдущий камень не долетел
                return;

            _lastUseTime = DateTime.Now;
            _target = target;
            _source = source;
            onStartUse?.Invoke();
        }

        public void OnOnCollisionEnter(IHealth health, float velocity)
        {
            var ratio = ((ISkilled)_source).GetSkillPower(this);
            health?.ChangeHP(-ratio * velocity, _source, this);
            _target = null;
        }

        public IThing Thing => _thing;

        public TimeSpan MinCastDuration => TimeSpan.FromSeconds(0.5f);

        public TimeSpan MaxCastDuration => TimeSpan.FromSeconds(3f);

        public TimeSpan? CastDuration
        {
            get
            {
                if (_beginCastTime == null)
                    return null;

                if (_endCastTime != null)
                    return _endCastTime.Value - _beginCastTime.Value;

                return DateTime.Now - _beginCastTime.Value;
            }
        }

        public float? CastDurationNormalized
        {
            get
            {
                if (CastDuration == null)
                    return null;

                return (float) CastDuration.Value.TotalSeconds / (float) MaxCastDuration.TotalSeconds;
            }
        }

        public void BeginCast()
        {
            _beginCastTime = DateTime.Now;
            IsCasting = true;
            OnBeginCast?.Invoke(this);
        }

        public void EndCast()
        {
            _endCastTime = DateTime.Now;
            IsCasting = false;
            OnEndCast?.Invoke(this);
        }

        public bool IsCasting { get; private set; }
        public event Action<ICastableSkill> OnBeginCast;
        public event Action<ICastableSkill> OnEndCast;
    }
}
