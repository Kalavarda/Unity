using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public abstract class AnimationManagerBase : IAnimationManager
    {
        private AnimationState _currentState;

        private static readonly IDictionary<int, IAnimationManager> _animationManagers = new Dictionary<int, IAnimationManager>();

        public static IAnimationManager CreateOrGet(GameObject gameObject)
        {
            var id = gameObject.GetInstanceID();
            if (!_animationManagers.ContainsKey(id))
            {
                var animator = gameObject.GetComponent<Animator>();
                if (animator.avatar.name.StartsWith("Male_") || animator.avatar.name.StartsWith("Female_"))
                    _animationManagers.Add(id, new Male_Female_AnimationManager(animator));
                else
                    throw new NotImplementedException();
            }

            return _animationManagers[id];
        }

        public void SetState(AnimationState state)
        {
            if (_currentState == state)
                return;
            _currentState = state;
            SetStateImpl(state);
        }

        public abstract void SetStateImpl(AnimationState state);
    }

    public class Male_Female_AnimationManager : AnimationManagerBase
    {
        private readonly Animator _animator;

        public Male_Female_AnimationManager([NotNull] Animator animator)
        {
            _animator = animator ?? throw new ArgumentNullException(nameof(animator));
        }

        public override void SetStateImpl(AnimationState state)
        {
            switch (state)
            {
                case AnimationState.Idle:
                    _animator.SetTrigger("OnIdle");
                    break;
                case AnimationState.GoForward:
                    _animator.SetTrigger("OnWalk");
                    break;
                case AnimationState.GoBack:
                    _animator.SetTrigger("OnIdle");
                    break;
                case AnimationState.Jump:
                    _animator.SetTrigger("OnJump");
                    break;
                case AnimationState.SimplePunch:
                    _animator.SetTrigger("OnAttack1");
                    break;
                case AnimationState.Die:
                    _animator.SetTrigger("OnDie");
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
