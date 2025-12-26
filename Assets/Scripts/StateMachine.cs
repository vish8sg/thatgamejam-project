using UnityEngine;

public class StateMachine
{
    IState currentState;

    public void ChangeState(IState newState)
    {
        if (currentState != null) { currentState.OnExit(); }
        currentState = newState;
        currentState.OnEnter();
    }

    public void Update()
    {
        if (currentState == null) { return; }
        currentState.OnUpdate();
    }

    public void FixedUpdate()
    {
        if (currentState == null) { return; }
        currentState.OnFixedUpdate();
    }
}