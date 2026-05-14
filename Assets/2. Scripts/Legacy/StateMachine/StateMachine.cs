using UnityEngine;

public class StateMachine : MonoBehaviour
{
    protected IState currentState;

    void Update()
    {
        currentState?.UpdateState();
    }
    
    public void ChangeState(IState newState)
    {
        currentState?.ExitState();
        currentState = newState;
        currentState?.EnterState();
    }
}
