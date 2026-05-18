using UnityEngine;

public class FloatingText : MonoBehaviour
{
    private Transform _mainCameraTransform;

    private void Awake()
    {
        if(Camera.main != null)
        {
            _mainCameraTransform = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if(_mainCameraTransform == null) return;

        transform.rotation = _mainCameraTransform.rotation;
    }
}
