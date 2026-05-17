using UnityEngine;

public class ConverterInput : MonoBehaviour
{
    private ResourceConverter _resourceConverter;

    void Start() 
    { 
        _resourceConverter = GetComponentInParent<ResourceConverter>(); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
            if(other.TryGetComponent<PlayerStacker>(out PlayerStacker playerStacker))
                _resourceConverter.SetPlayerInInput(playerStacker);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) 
            _resourceConverter.SetPlayerInInput(null);
    }
}