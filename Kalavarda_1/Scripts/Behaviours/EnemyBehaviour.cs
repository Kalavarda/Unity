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
            var playerGameObject = PlayerBehaviour.Instance.PlayerGameObject;

            var distance = Utils.Distance(playerGameObject, gameObject);

            // TODO: не глючит только если моб лицом к player'у
            var angle = Vector3.Angle(gameObject.transform.forward, playerGameObject.transform.forward) / 180;

            var skillContext = new SkillContext(_player, distance, angle);

            var availableSkills = skilled.Skills.Where(sk => sk.ReadyToUse(skillContext)).ToArray();
            if (availableSkills.Any())
            {
                // поворачиваем моба в сторону player'а
                gameObject.transform.LookAt(playerGameObject.transform);

                var skill = availableSkills[_rand.Next(availableSkills.Length)];
                if (skill != null)
                    skilled.Use(skill, skillContext, () =>
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
