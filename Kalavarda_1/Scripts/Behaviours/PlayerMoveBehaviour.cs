using System;
using Assets.Scripts.Model;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Behaviours
{
    public class PlayerMoveBehaviour: MonoBehaviour
    {
        public Transform Player;
        public Animator PlayerAnimator;
        public Transform CenterPoint;
        public float MoveSpeed = 1.388f;
        public float RotationSpeed = 6.28f;
        public bool RotatePlayerByWASD = true;

        public static PlayerMoveBehaviour Instance { get; private set; }

        public IAnimationManager AnimationManager { get; private set; }

        private float _walkForce;
        private float _jumpImpulse;
        private Rigidbody _rigidbody;
        private readonly TimeIntervalLimiter _jumpLimiter = new TimeIntervalLimiter(TimeSpan.FromSeconds(1));
        private bool jumping;

        private Vector3 _lastCollisionImpulse;
        private readonly Player _player = Model.Player.Instance;

        void Start()
        {
            Instance = this;

            _rigidbody = Player.GetComponent<Rigidbody>();
            _walkForce = _rigidbody.mass * 500;
            _jumpImpulse = _rigidbody.mass * 5f;
            if (PlayerAnimator != null)
                AnimationManager = AnimationManagerBase.CreateOrGet(PlayerAnimator.gameObject);
        }

        void Update()
        {
            var w = Input.GetKey(KeyCode.W);
            var a = Input.GetKey(KeyCode.A);
            var s = Input.GetKey(KeyCode.S);
            var d = Input.GetKey(KeyCode.D);

            if (Input.GetMouseButton((int)MouseButton.LeftMouse))
                if (!Input.GetMouseButton((int) MouseButton.RightMouse))
                {
                    var playerAngle = CenterPoint.eulerAngles.y;
                    if (RotatePlayerByWASD)
                        playerAngle = GetPlayerAngle(playerAngle, w, a, s, d);
                    var rot = Quaternion.Euler(0, playerAngle, 0);
                    _rigidbody.MoveRotation(Quaternion.Lerp(_rigidbody.rotation, rot, RotationSpeed * Time.deltaTime));
                }

            if (w || a || s || d)
                Move(MoveSpeed * _player.Characteristics.SpeedRatio.Value, w, a, s, d);
            else
                AnimationManager?.SetState(AnimationState.Idle);
            
            if (Input.GetKeyDown(KeyCode.Space))
                Jump();
        }

        private void Jump()
        {
            if (jumping)
                return;

            _jumpLimiter.Do(() =>
            {
                _rigidbody.AddForce(Player.up * _jumpImpulse, ForceMode.Impulse);
                jumping = true;
            });
        }

        private void Move(float maxSpeed, bool w, bool a, bool s, bool d)
        {
            if (jumping)
                return;

            var dy = _lastCollisionImpulse.y - 13.7f; // todo: magic number
            if (dy > 0)
                _rigidbody.AddForce(Player.up * 100f, ForceMode.Force); // todo: magic number (need Time.deltaTime?)

            if (_rigidbody.velocity.magnitude < maxSpeed)
            {
                var direction = GetDirection(Player, w, a, s, d);
                _rigidbody.AddForce(direction * _walkForce * Time.deltaTime, ForceMode.Force);
            }

            AnimationManager?.SetState(AnimationState.GoForward);
        }

        private Vector3 GetDirection(Transform player, bool w, bool a, bool s, bool d)
        {
            if (RotatePlayerByWASD)
                return player.forward;

            if (w)
            {
                if (a)
                    return Vector3.Lerp(player.forward, -player.right, 0.5f);
                if (d)
                    return Vector3.Lerp(player.forward, player.right, 0.5f);
                return player.forward;
            }

            if (s)
            {
                if (a)
                    return Vector3.Lerp(-player.forward, -player.right, 0.5f);
                if (d)
                    return Vector3.Lerp(-player.forward, player.right, 0.5f);
                return -player.forward;
            }

            if (d)
                return player.right;

            if (a)
                return -player.right;

            throw new NotImplementedException();
        }

        void OnCollisionEnter(Collision collision)
        {
            jumping = false;
        }

        void OnCollisionStay(Collision collision)
        {
            _lastCollisionImpulse = collision.impulse;
        }

        void OnCollisionExit(Collision collision)
        {
        }

        private float GetPlayerAngle(float startAngle, bool w, bool a, bool s, bool d)
        {
            var result = startAngle;

            if (w)
            {
                if (a)
                    result -= 45;
                if (d)
                    result += 45;
            }
            else
            {
                if (s)
                {
                    if (a)
                        result -= 135;
                    if (d)
                        result += 135;
                    if (!a && !d)
                        result += 180;
                }
                else
                {
                    if (a)
                        result -= 90;
                    if (d)
                        result += 90;
                }
            }

            return result;
        }

        public void GoPlayerTo(float x, float z)
        {
            //Player.position = new Vector3(x, 5, z);
            Player.SetPositionAndRotation(new Vector3(x, 5, z), Player.rotation);
        }
    }
}
