using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    
    public void AnimationStart(int stateHash, int stateNumber, int thresholdHash, float thresholdValue)
    {
        _animator.SetInteger(stateHash, stateNumber);
        _animator.SetFloat(thresholdHash, thresholdValue);
    }
}
