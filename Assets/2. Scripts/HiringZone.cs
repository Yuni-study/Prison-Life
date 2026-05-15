using UnityEngine;
using System.Collections;

public class HiringZone : MonoBehaviour
{
    public int hireCost = 1000;
    public int currentPaid = 0;
    public GameObject prisonerGroup; 

    public ZoneUI zoneUI; // UI 스크립트 연결
    
    private bool isPlayerInside = false;
    private PlayerStacker playerStacker;
    private Coroutine paymentCoroutine;

    private void Start()
    {
        if(prisonerGroup != null) prisonerGroup.SetActive(false);
        if(zoneUI != null) zoneUI.UpdateUI(currentPaid, hireCost);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            playerStacker = other.GetComponent<PlayerStacker>();
            
            // 이미 코루틴이 돌고 있지 않을 때만 시작
            if (paymentCoroutine == null)
            {
                paymentCoroutine = StartCoroutine(PaymentRoutine());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            // 구역을 나가면 지불 중단
            if (paymentCoroutine != null)
            {
                StopCoroutine(paymentCoroutine);
                paymentCoroutine = null;
            }
        }
    }

    IEnumerator PaymentRoutine()
    {
        // 1. 구역에 들어오자마자 바로 뺏지 않도록 약간의 대기시간을 줍니다 (0.5초)
        yield return new WaitForSeconds(0.5f);

        while (isPlayerInside && currentPaid < hireCost)
        {
            if (playerStacker != null)
            {
                GameObject money = playerStacker.PopSpecificItem("Money");
                
                if (money != null)
                {
                    playerStacker.AddMoney(-100); // 돈 하나당 100원으로 가정
                    // 돈이 날아가는 연출 (기존 UpgradeZone의 FlyToZone을 여기도 복사해서 사용 권장)
                    StartCoroutine(FlyToZone(money));
                    
                    currentPaid += 100; // 돈 한 장당 가치

                    // UI 업데이트
                    if(zoneUI != null) zoneUI.UpdateUI(currentPaid, hireCost);
                    
                    if (currentPaid >= hireCost)
                    {
                        CompleteHiring();
                        yield break;
                    }
                }
            }
            
            // 2. 지불 간격을 조절합니다 (0.2초마다 하나씩 지불)
            // 이 값이 너무 작으면 순식간에 다 사라집니다.
            yield return new WaitForSeconds(0.2f);
        }
        
        paymentCoroutine = null;
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

    void CompleteHiring()
    {
        if (prisonerGroup != null) prisonerGroup.SetActive(true);
        gameObject.SetActive(false); 
        Debug.Log("죄수 고용 완료!");
    }
}