using System.Linq;
using Assets.Scripts.Model;
using Assets.Scripts.Utils;
using UnityEngine;
using Random = System.Random;

public class EnemyBehaviour : MonoBehaviour
{
    private ISpawned _thisEnemy;
    private DpsMeter _dpsMeter;
    private static readonly Random _rand = new Random();
    private readonly Player _player = Player.Instance;

    void Start()
    {
        _thisEnemy = SpawnerBehaviour.AllSpawned[gameObject];
        _dpsMeter = HUDBehaviour.Instance.DpsMeter;

        if (_thisEnemy is IFighter fighter)
            fighter.DagameReceived += damage => _dpsMeter.CurrentFight.AddDamage(damage);
    }

    void Update()
    {
        if (_thisEnemy is IHealth health)
        {
            if (health.IsDied)
                return;
            health.Update();
        }

        if (_thisEnemy is ISkilled skilled)
        {
            var distance = Utils.Distance(PlayerBehaviour.Instance.PlayerGameObject, gameObject);

            var availableSkills = skilled.Skills.Where(sk => sk.ReadyToUse(_player, distance)).ToArray();
            if (availableSkills.Any())
            {
                var skill = availableSkills[_rand.Next(availableSkills.Length)];
                if (skill != null)
                {
                    skilled.Use(skill, _player, distance, () =>
                    {
                        var animState = AnimationAttribute.GetAnimationState(skill);
                        if (animState != null)
                            AnimationManagerBase.CreateOrGet(gameObject).SetState(animState.Value);

                        SkillBehaviour.GetAudioSource(skill)?.Play();
                    });
                }
            }
        }
    }
}
