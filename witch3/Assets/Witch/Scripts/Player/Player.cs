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
        const float speed = 1f;

        var dir = _inputActions.Player.Move.ReadValue<Vector2>();
        transform.Translate(dir * speed * Time.deltaTime);

        _stateMachine.Update();

        Debug.Log(_stateMachine.CurrentStateName);
    }

    private class StateBase : ImtStateMachine<Player, StateEvent>.State { }

    private class IdleState : StateBase
    {
        protected override void Enter()
        {

        }

        protected override void Update()
        {

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
            var rigidBody = Context.GetComponent<Rigidbody2D>();
            var vel = rigidBody.velocity;
            if(vel.y == 0.0f)
            {
                Context._stateMachine.SendEvent(StateEvent.Idle);
            }
        }
    }

    ImtStateMachine<Player, StateEvent> _stateMachine;

    WitchInput _inputActions = null;
    
}
