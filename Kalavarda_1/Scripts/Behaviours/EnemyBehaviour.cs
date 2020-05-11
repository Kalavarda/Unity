using System;
using System.Linq;
using Assets.Scripts.Model;
using Assets.Scripts.Utils;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private ISpawned _thisEnemy;
    private DpsMeter _dpsMeter;

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
            var skill = skilled.Skills.FirstOrDefault(sk => sk.ReadyToUse); // сделать рандомную выборку
            if (skill != null)
            {
                var distance = Utils.Distance(PlayerBehaviour.Instance.PlayerGameObject, gameObject);
                skilled.Use(skill, Player.Instance, distance, () =>
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
