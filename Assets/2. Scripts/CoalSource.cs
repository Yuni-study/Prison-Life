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
                if (stacker.IsFull()) 
                {
                    stacker.ShowMaxText();
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    GameObject newCoal = Instantiate(coalPrefab);
                    stacker.AddItemToList(newCoal, "Coal");
                }
            }
            // UpgradeManager에서 현재 속도를 가져옴
            yield return new WaitForSeconds(UpgradeManager.Instance.GetCurrentMineInterval());
        }
    }
}