using UnityEngine;

public class IdleState : State
{
    private FSMAgent _agent;
    float _timeToChangePatrol = 3f;

    float _timer = 0f;

    public IdleState(FSMAgent agent) : base(agent.FSM) 
    {
        _agent = agent;
    }
    
    public override void Enter()
    {
        Debug.LogError("Entre a idle");
        _timer = 0f;
    }

    public override void Update()
    {
        _timer += Time.deltaTime;
        if(_timer >= _timeToChangePatrol)
        {
            _agent.FSM.ChangeState(_agent.Patrol);
        }
    }
    public override void Exit()
    {
        Debug.LogError("Sali idle");
    }

}
