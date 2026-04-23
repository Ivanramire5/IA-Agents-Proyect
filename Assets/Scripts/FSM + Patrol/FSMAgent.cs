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

    private void Start() 
    {
        IdleState idle = new();
        PatrolState patrol = new(_patrolData, this);
        _fsm.ChangeState(patrol);
    }

    private void Update()
    {
        _fsm.Update();
    }

    
}
