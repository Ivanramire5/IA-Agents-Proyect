using UnityEngine;

public class IdleState : State
{
    public override void Enter()
    {
        Debug.LogError("Entre a idle");
    }

    public override void Update()
    {
        Debug.LogError("Estoy en idle");
    }
    public override void Exit()
    {
        Debug.LogError("Sali idle");
    }

}
