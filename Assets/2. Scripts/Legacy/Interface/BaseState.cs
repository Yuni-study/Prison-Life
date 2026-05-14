using UnityEngine;

public abstract class BaseState : IState
{
    protected AnimationController animationController;

    public BaseState(PlayerContext playerContext)
    {
        animationController = playerContext.animationController;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();

    public virtual void OnTriggerEnterState(Collider other)
    {
        
    }

    public virtual void OnTriggerExitState(Collider other)
    {
        
    }
}
