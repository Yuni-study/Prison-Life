using UnityEngine;

public class ConverterInput : MonoBehaviour
{
    private ResourceConverter master;

    void Start() { master = GetComponentInParent<ResourceConverter>(); }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
            if(other.TryGetComponent<PlayerStacker>(out PlayerStacker stacker))
                master.SetPlayerInInput(stacker);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) 
            master.SetPlayerInInput(null);
    }
}