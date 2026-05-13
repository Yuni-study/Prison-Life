using UnityEngine;

public class PlayerIdleState : GroundState
{
    public PlayerIdleState(PlayerContext playerContext) : base(playerContext) {}

    public override void EnterState()
    {
        base.EnterState();
        Debug.Log("Idle 상태 진입");

        animationController.AnimationStart(HashValues.STATE, 0, HashValues.THRESHOLD, 0.0f);
    }

    public override void UpdateState()
    {
        base.UpdateState();
    }

    public override void ExitState()
    {
        base.ExitState();
    }
}
