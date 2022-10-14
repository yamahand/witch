using System.Collections;
using IceMilkTea.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using Witch.Unit;

namespace Witch.Player
{
    public partial class Player : MonoBehaviour
    {
        public bool IsDashing => _stateMachine.IsCurrentState<DashState>();
    
        enum StateEvent
        {
            Idle,
            Jump,
            Dash,
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _ground = GetComponent<Ground>();
        }

        // Start is called before the first frame update
        void Start()
        {
            _inputActions = new WitchInput();
            _inputActions.Enable();
            _inputActions.Player.Fire.performed += _ => Debug.Log("fire performed");
            _inputActions.Player.Fire.canceled += _ => Debug.Log("fire canceled");
            _inputActions.Player.Fire.started += _ => Debug.Log("fire started");
            _inputActions.Player.Jump.performed += _ => _stateMachine.SendEvent(StateEvent.Jump);
            _inputActions.Player.Dash.performed += _ => _stateMachine.SendEvent(StateEvent.Dash); 

            _stateMachine = new ImtStateMachine<Player, StateEvent>(this);
            _stateMachine.AddAnyTransition<IdleState>(StateEvent.Idle);
            _stateMachine.AddTransition<IdleState, JumpState>(StateEvent.Jump);
            _stateMachine.AddTransition<IdleState, DashState>(StateEvent.Dash);
            _stateMachine.AddTransition<JumpState, DashState>(StateEvent.Dash);

            _stateMachine.SetStartState<IdleState>();
            _stateMachine.Update();
        
            InitDebugText();
        }

        // Update is called once per frame
        void Update()
        {
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
            _onGround = _ground.OnGround;
            _velocity = _rigidbody.velocity;

            _acceleration = _onGround ? _maxAcceleration : _maxAirAcceleration;
            if (IsDashing)
            {
                _acceleration = _desiredVelocity.x;
            }
            _maxSpeedChange = _acceleration * Time.deltaTime;
            _velocity.x = Mathf.MoveTowards(_velocity.x, _desiredVelocity.x, _maxSpeedChange);

            _rigidbody.velocity = _velocity;
        }

        private class StateBase : ImtStateMachine<Player, StateEvent>.State { }

        private class IdleState : StateBase
        {
            protected override void Enter()
            {

            }

            protected override void Update()
            {
                Context._direction = Context._inputActions.Player.Move.ReadValue<Vector2>();
                Context._desiredVelocity = new Vector2(Context._direction.x, 0f) * Mathf.Max(Context._maxSpeed - Context._ground.Friction, 0f);
                if (Context._direction.x != 0f)
                {
                    Context.transform.localScale = new Vector3(Mathf.Sign(Context._direction.x), 1f, 1f);
                }
            }
        }

        private class JumpState : StateBase
        {
            protected override void Enter()
            {
                var rigidBody = Context._rigidbody;
                var vel = rigidBody.velocity;
                vel.y += Context._jumpPower;
                rigidBody.velocity = vel;
            }

            protected override void Update()
            {
                Context._direction = Context._inputActions.Player.Move.ReadValue<Vector2>();
                Context._desiredVelocity = new Vector2(Context._direction.x, 0f) * Mathf.Max(Context._maxSpeed - Context._ground.Friction, 0f);
                if (Context._direction.x != 0f)
                {
                    Context.transform.localScale = new Vector3(Mathf.Sin(Context._direction.x), 1f, 1f);
                }
                if(Context._onGround && Context._rigidbody.velocity.y <= 0f)
                {
                    Context._stateMachine.SendEvent(StateEvent.Idle);
                }
            }
        }

        private class DashState : StateBase
        {
            protected override void Enter()
            {
                _dashDir = Context._direction;
                if (_dashDir.x == 0f)
                {
                    _dashDir.x = Context.transform.localScale.x;
                }
                Context._desiredVelocity = new Vector2(_dashDir.x, 0f) * Mathf.Max(Context._dashSpeed, 0f);
            }

            protected override void Update()
            {
                Context.StartCoroutine(StopDashing());
            }

            private IEnumerator StopDashing()
            {
                yield return new WaitForSeconds(Context._dashTime);
                Context._stateMachine.SendEvent(StateEvent.Idle);
            }
        
            private Vector2 _dashDir;
        }

        ImtStateMachine<Player, StateEvent> _stateMachine;

        WitchInput _inputActions;

        [FoldoutGroup("移動"),SerializeField, Range(1f, 100f)] private float _maxSpeed = 4f;
        [FoldoutGroup("移動"),SerializeField, Range(1f, 100f)] private float _maxAcceleration = 35f;
        [FoldoutGroup("移動"),SerializeField, Range(1f, 100f)] private float _maxAirAcceleration = 20f;
        [FoldoutGroup("移動"),SerializeField, Range(1f, 100f)] private float _jumpPower = 10.0f;
    
        [FoldoutGroup("ダッシュ"), SerializeField, Range(1f, 100f)] private float _dashSpeed = 14f;
        [FoldoutGroup("ダッシュ"), SerializeField, Range(0.01f, 10f)] private float _dashTime = 0.5f;
        private bool _isDashing;
        private bool _canDash;

        private Vector2 _direction = Vector2.zero;
        private Vector2 _desiredVelocity = Vector2.zero;
        private Vector2 _velocity = Vector2.zero;
        private Rigidbody2D _rigidbody;
        private Ground _ground;
        private float _maxSpeedChange;
        private float _acceleration;
        private bool _onGround;
    }
}
