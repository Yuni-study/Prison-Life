using UnityEngine;
using System.Collections;

public class CoalSource : MonoBehaviour
{
    [SerializeField] private GameObject coalPrefab; // 생성할 석탄 프리팹
    [SerializeField] private float mineInterval = 0.5f; // 채굴 속도
    private bool isMining = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isMining = true;
            StartCoroutine(MiningCoroutine(other.GetComponent<PlayerStacker>()));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isMining = false;
        }
    }

    IEnumerator MiningCoroutine(PlayerStacker stacker)
    {
        while (isMining)
        {
            if (stacker != null)
            {
                // 플레이어에게 석탄을 추가해달라고 요청
                bool success = stacker.AddResource(coalPrefab);
                
                // 만약 꽉 찼다면 잠시 대기하거나 루프를 멈출 수 있음
                if (!success) yield return new WaitForSeconds(1f); 
            }
            yield return new WaitForSeconds(mineInterval);
        }
    }
}