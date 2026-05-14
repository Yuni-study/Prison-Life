using UnityEngine;

public class ConverterInput : MonoBehaviour
{
    private ResourceConverter master;

    void Start() { master = GetComponentInParent<ResourceConverter>(); }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
            master.SetPlayerInInput(other.GetComponent<PlayerStacker>());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) 
            master.SetPlayerInInput(null);
    }
}