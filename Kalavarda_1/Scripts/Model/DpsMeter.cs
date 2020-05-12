using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Assets.Scripts.Model
{
    public class DpsMeter
    {
        public FightInfo CurrentFight => _fights.FirstOrDefault();

        public IReadOnlyCollection<FightInfo> AllFights => _fights;

        private readonly List<FightInfo> _fights = new List<FightInfo>();

        public FightInfo StartFight()
        {
            var fight = new FightInfo();
            _fights.Insert(0, fight);

            if (_fights.Count > 10)
                _fights.Remove(_fights.Last());

            return fight;
        }
    }

    public class FightInfo
    {
        private readonly ICollection<Tuple<DamageInfo, DateTime>> _damages = new List<Tuple<DamageInfo, DateTime>>();

        public TimeSpan Duration
        {
            get
            {
                if (!_damages.Any())
                    return TimeSpan.Zero;

                return _damages.Max(t => t.Item2) - _damages.Min(t => t.Item2);
            }
        }

        public float GetDpsOf([NotNull] IHealth damager)
        {
            if (damager == null) throw new ArgumentNullException(nameof(damager));

            var damages = _damages.Where(t => t.Item1.From == damager).Select(t => t.Item1).ToArray();
            if (!damages.Any())
                return 0;
            if (Duration == TimeSpan.Zero)
                return damages.Sum(d => d.Damage);
            return damages.Sum(d => d.Damage) / (float)Duration.TotalSeconds;
        }

        public IReadOnlyDictionary<ISkill, float> GetDetailedDpsOf([NotNull] IHealth damager)
        {
            if (damager == null) throw new ArgumentNullException(nameof(damager));

            var result = new Dictionary<ISkill, float>();

            var damages = _damages.Where(t => t.Item1.From == damager).Select(t => t.Item1).ToArray();
            if (!damages.Any())
                return result;

            foreach (var damageInfo in damages)
            {
                if (!result.ContainsKey(damageInfo.Skill))
                    result.Add(damageInfo.Skill, 0);
                result[damageInfo.Skill] += damageInfo.Damage;
            }

            if (Duration != TimeSpan.Zero)
            {
                var seconds = (float)Duration.TotalSeconds;
                var skills = result.Keys.ToArray();
                foreach (var skill in skills)
                    result[skill] = result[skill] / seconds;
            }

            return result;
        }

        public void AddDamage([NotNull] DamageInfo damageInfo)
        {
            if (damageInfo == null) throw new ArgumentNullException(nameof(damageInfo));
            _damages.Add(new Tuple<DamageInfo, DateTime>(damageInfo, DateTime.Now));
        }
    }

    public class DamageInfo
    {
        public ISkilled From { get; }

        public IHealth To { get; }

        public ISkill Skill { get; }

        public float Damage { get; }

        public DamageInfo([NotNull] ISkilled from, [NotNull] IHealth to, [NotNull] ISkill skill, float damage)
        {
            From = @from ?? throw new ArgumentNullException(nameof(@from));
            To = to ?? throw new ArgumentNullException(nameof(to));
            Skill = skill ?? throw new ArgumentNullException(nameof(skill));
            Damage = damage;
        }

        public override string ToString()
        {
            return Skill.GetType().Name + " " + Math.Round(Damage, 1);
        }
    }
}
