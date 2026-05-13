using UnityEngine;

public class PlayerContext : MonoBehaviour
{
    public AnimationController animationController { get; private set;}

    private void Start()
    {
        animationController = GetComponentInChildren<AnimationController>();
    }
}
