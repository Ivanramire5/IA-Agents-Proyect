using UnityEngine;


//Colocamos una T para que sea de tipo generico y pueda ser leido 
//sin importar el tipo de dato
public abstract class State
{
    protected readonly FiniteStateMachine _fsm;

    public State(FiniteStateMachine fms)
    {
        _fsm = fms;
    }
    public abstract void Enter();

    public abstract void Update();

    public abstract void Exit();
}
