using UnityEngine;

public class FiniteStateMachine
{
    private State _currentState;

    public void ChangeState(State newState)
    {
        _currentState?.Exit();

        _currentState = newState;

        _currentState.Enter();
    }

    public void Update()
    {
        _currentState?.Update();
    }

    
}
