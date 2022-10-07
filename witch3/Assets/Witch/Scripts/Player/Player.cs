using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IceMilkTea.Core;

public partial class Player : MonoBehaviour
{
    enum StateEvent
    {
        Idle,
        Jump,
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
        _inputActions.Player.Fire.performed += context => Debug.Log("fire performed");
        _inputActions.Player.Fire.canceled += context => Debug.Log("fire canceled");
        _inputActions.Player.Fire.started += context => Debug.Log("fire started");
        _inputActions.Player.Jump.performed += context => _stateMachine.SendEvent(StateEvent.Jump);

        _stateMachine = new ImtStateMachine<Player, StateEvent>(this);
        _stateMachine.AddAnyTransition<IdleState>(StateEvent.Idle);
        _stateMachine.AddTransition<IdleState, JumpState>(StateEvent.Jump);

        _stateMachine.SetStartState<IdleState>();
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
        }
    }

    private class JumpState : StateBase
    {
        protected override void Enter()
        {
            var rigidBody = Context.GetComponent<Rigidbody2D>();
            var vel = rigidBody.velocity;
            vel.y += 10.0f;
            rigidBody.velocity = vel;
        }

        protected override void Update()
        {
            if(Context._onGround)
            {
                Context._stateMachine.SendEvent(StateEvent.Idle);
            }
        }
    }

    ImtStateMachine<Player, StateEvent> _stateMachine;

    WitchInput _inputActions = null;

    [SerializeField, Range(0f, 100f)] private float _maxSpeed = 4f;
    [SerializeField, Range(0f, 100f)] private float _maxAcceleration = 35f;
    [SerializeField, Range(0f, 100f)] private float _maxAirAcceleration = 20f;

    private Vector2 _direction = Vector2.zero;
    private Vector2 _desiredVelocity = Vector2.zero;
    private Vector2 _velocity = Vector2.zero;
    private Rigidbody2D _rigidbody = null;
    private Ground _ground = null;
    private float _maxSpeedChange = 0.0f;
    private float _acceleration = 0.0f;
    private bool _onGround = false;
}
