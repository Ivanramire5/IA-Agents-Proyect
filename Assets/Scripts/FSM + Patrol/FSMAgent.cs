using System.Collections.Generic;
using UnityEngine;

public class FSMAgent : MonoBehaviour
{

    [SerializeField]
    private float _speed = 3f;
    public float Speed => _speed;

    
    [SerializeField]
    private PatrolData _patrolData;

    private readonly FiniteStateMachine _fsm = new();

    public FiniteStateMachine FSM => _fsm;

    public IdleState Idle { get; private set; }

    public PatrolState Patrol { get; private set; }

    private void Start() 
    {
        Idle = new(this);
        Patrol = new(_patrolData, this);
        _fsm.AddState(Idle);
        _fsm.AddState(Patrol);
        _fsm.ChangeState(Idle);
    }

    
    private void Update()
    {
        _fsm.Update();
    }

    
}
