using UnityEngine;

public interface IState 
{
    public void EnterState();
    public void UpdateState();
    public void ExitState();
    public void OnTriggerEnterState(Collider other);
    public void OnTriggerExitState(Collider other);
}
