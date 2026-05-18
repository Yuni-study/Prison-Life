using UnityEngine;
using System.Collections;

public class UpgradeZone : MonoBehaviour
{
    public enum UpgradeType { MineSpeed, Capacity }
    public UpgradeType type;

    [Header("Cost Settings")]
    public int upgradeCost = 500;
    public int currentPaid = 0; // 현재까지 지불된 금액
    public ZoneUI zoneUI; // UI 스크립트 연결

    private bool _isPlayerInside = false;
    private PlayerStacker playerStacker;

    private void Start()
    {
        // 시작할 때 UI 초기화
        if(zoneUI != null) zoneUI.UpdateUI(currentPaid, upgradeCost);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInside = true;
            playerStacker = other.GetComponent<PlayerStacker>();
            StartCoroutine(_PaymentRoutine());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInside = false;
        }
    }

    private IEnumerator _PaymentRoutine()
    {
        while (_isPlayerInside && currentPaid < upgradeCost)
        {
            // 플레이어 뒤에 돈(Money)이 있는지 확인
            GameObject moneyItem = playerStacker.PopSpecificItem("Money");

            // if(moneyItem == null) Debug.Log("돈 없음");
            // else Debug.Log("돈 있음");

            if (moneyItem != null)
            {
                playerStacker.AddMoney(-100); // 돈 하나당 100원으로 가정
                // 돈이 업그레이드 패드로 날아가는 연출
                StartCoroutine(_FlyToZone(moneyItem));
                currentPaid += 100; // 돈 하나당 가치 (기획에 따라 수정)

                // UI 업데이트
                if(zoneUI != null) zoneUI.UpdateUI(currentPaid, upgradeCost);

                if (currentPaid >= upgradeCost)
                {
                    _CompleteUpgrade();
                    yield break;
                }
            }
            
            yield return new WaitForSeconds(0.1f); // 지불 속도
        }
    }

    private IEnumerator _FlyToZone(GameObject money)
    {
        float elapsed = Constants.ZERO_FLOAT;
        float duration = Constants.POINTTHREE;
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

    private void _CompleteUpgrade()
    {
        // 이제 Manager에서 색상, 범위, 소지량을 한꺼번에 처리합니다.
        UpgradeManager.Instance.ProcessUpgrade();

        // 다음 업그레이드를 위한 초기화
        currentPaid = Constants.ZERO_INTEGER;
        
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