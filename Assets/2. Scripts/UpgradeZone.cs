using UnityEngine;
using System.Collections;

public class UpgradeZone : MonoBehaviour
{
    public enum UpgradeType { MineSpeed, Capacity }
    public UpgradeType type;

    [Header("Cost Settings")]
    public int upgradeCost = 500;
    public int currentPaid = 0; // 현재까지 지불된 금액

    private bool isPlayerInside = false;
    private PlayerStacker playerStacker;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            playerStacker = other.GetComponent<PlayerStacker>();
            StartCoroutine(PaymentRoutine());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }

    IEnumerator PaymentRoutine()
    {
        while (isPlayerInside && currentPaid < upgradeCost)
        {
            // 플레이어 뒤에 돈(Money)이 있는지 확인
            GameObject moneyItem = playerStacker.PopSpecificItem("Money");

            // if(moneyItem == null) Debug.Log("돈 없음");
            // else Debug.Log("돈 있음");

            if (moneyItem != null)
            {
                // 돈이 업그레이드 패드로 날아가는 연출
                StartCoroutine(FlyToZone(moneyItem));
                currentPaid += 100; // 돈 하나당 가치 (기획에 따라 수정)

                if (currentPaid >= upgradeCost)
                {
                    CompleteUpgrade();
                    yield break;
                }
            }
            
            yield return new WaitForSeconds(0.1f); // 지불 속도
        }
    }

    IEnumerator FlyToZone(GameObject money)
    {
        float elapsed = 0f;
        float duration = 0.3f;
        Vector3 startPos = money.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            money.transform.position = Vector3.Lerp(startPos, transform.position, elapsed / duration);
            money.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, elapsed / duration);
            yield return null;
        }

        Destroy(money);
    }

    void CompleteUpgrade()
    {
        // 이제 Manager에서 색상, 범위, 소지량을 한꺼번에 처리합니다.
        UpgradeManager.Instance.ProcessUpgrade();

        // 다음 업그레이드를 위한 초기화
        currentPaid = 0;
        
        // 2단계가 끝났다면 존을 비활성화하거나 텍스트를 "MAX"로 변경
        if (UpgradeManager.Instance.upgradeLevel >= 2)
        {
            gameObject.SetActive(false); 
            Debug.Log("모든 업그레이드 완료!");
        }
        else
        {
            upgradeCost += 1000; // 비용 상승
        }
    }
}