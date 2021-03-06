﻿using System;
using Assets.Scripts.Model;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.AI;
using AnimationState = Assets.Scripts.AnimationState;
using Random = System.Random;
using Utils = Assets.Scripts.Utils.Utils;

public class NavigationBehaviour : MonoBehaviour
{
    public bool PursueMode = false;

    private NavMeshAgent _agent;
    private static readonly Random _rand = new Random();

    private readonly TimeIntervalLimiter _limiter = new TimeIntervalLimiter(TimeSpan.FromSeconds(0.5));
    private readonly TimeIntervalLimiter _randomMoveLimiter = new TimeIntervalLimiter(TimeSpan.FromMinutes(0.25 + 0.5 * _rand.NextDouble()));

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        _limiter.Do(() =>
        {
            if (SpawnerBehaviour.AllSpawned.TryGetValue(gameObject, out var spawned))
            {
                if (spawned is IHealth health)
                    if (health.IsDied)
                        return;
                if (spawned is IEnemy enemy)
                    Navigate(enemy, PlayerBehaviour.Instance.PlayerGameObject, Player.Instance);
            }
        });
    }

    private void Navigate(IEnemy enemy, GameObject playerGameObject, IHealth player)
    {
        if (enemy is IHealth health)
            if (health.IsDied)
            {
                _agent.isStopped = true;
                return;
            }

        var animMaganer = AnimationManagerBase.CreateOrGet(gameObject);

        // агриться, только если не сзади
        if (enemy.AggressionTarget == null)
        {
            var distance = Utils.Distance(gameObject, playerGameObject);
            if (distance < enemy.AggressionDistance && distance > _agent.stoppingDistance)
            {
                var angle = Vector3.Angle(gameObject.transform.forward, playerGameObject.transform.forward);
                if (angle > 45) // конус 90 градусов сзади
                    enemy.AggressionTarget = player;
            }
        }

        if (enemy.AggressionTarget == player)
        {
            _agent.isStopped = false;
            animMaganer.SetState(AnimationState.GoForward);
            if (PursueMode)
                _agent.SetDestination(playerGameObject.transform.position);
            else
            {
                var d = gameObject.transform.position - playerGameObject.transform.position;
                var newX = gameObject.transform.position.x + d.x / 2;
                var newY = gameObject.transform.position.y + d.y / 2;
                var newZ = gameObject.transform.position.z + d.z / 2;
                _agent.SetDestination(new Vector3(newX, newY, newZ));
            }
        }
        else
        {
            _randomMoveLimiter.Do(() =>
            {
                //if (!_agent.isStopped)
                //    return;

                _agent.isStopped = false;
                animMaganer.SetState(AnimationState.GoForward);

                var randomVector = SpawnerBehaviour.GetRandomVector(new Vector3(1, 0, 1)) * (float)_rand.NextDouble() * enemy.AggressionDistance;
                animMaganer.SetState(AnimationState.GoForward);
                var target = gameObject.transform.position + randomVector;
                _agent.SetDestination(target);
            }, () =>
            {
                //if (_agent.pathStatus == NavMeshPathStatus.PathComplete)
                if (_agent.remainingDistance <= _agent.stoppingDistance)
                {
                    _agent.isStopped = true;
                    animMaganer.SetState(AnimationState.Idle);
                }
            });
        }
    }
}
