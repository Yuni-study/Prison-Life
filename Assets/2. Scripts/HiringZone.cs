using UnityEngine;
using System.Collections;

public class HiringZone : MonoBehaviour
{
    public int hireCost = 1000;
    public int currentPaid = 0;
    public GameObject prisonerGroup; 

    public ZoneUI zoneUI; 
    
    private bool isPlayerInside = false;
    private PlayerStacker _playerStacker;
    private Coroutine _paymentCoroutine;

    private WaitForSeconds _waitDuration1;
    private WaitForSeconds _waitDuration2;

    private void Awake()
    {
        _waitDuration1 = new WaitForSeconds(Constants.POINTFIVE);
        _waitDuration2 = new WaitForSeconds(Constants.POINTTWO);
    }

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
            _playerStacker = other.GetComponent<PlayerStacker>();
            
            // 이미 코루틴이 돌고 있지 않을 때만 시작
            if (_paymentCoroutine == null)
            {
                _paymentCoroutine = StartCoroutine(_PaymentRoutine());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;

            // 구역을 나가면 지불 중단
            if (_paymentCoroutine != null)
            {
                StopCoroutine(_paymentCoroutine);
                _paymentCoroutine = null;
            }
        }
    }

    private IEnumerator _PaymentRoutine()
    {
        // 1. 구역에 들어오자마자 바로 뺏지 않도록 약간의 대기시간을 줍니다 (0.5초)
        yield return _waitDuration1;

        while (isPlayerInside && currentPaid < hireCost)
        {
            if (_playerStacker != null)
            {
                GameObject money = _playerStacker.PopSpecificItem("Money");
                
                if (money != null)
                {
                    _playerStacker.AddMoney(-100); // 100원 차감 

                    // 돈이 날아가는 연출 
                    StartCoroutine(_FlyToZone(money));
                    
                    currentPaid += 100; 

                    // UI 업데이트
                    if(zoneUI != null) zoneUI.UpdateUI(currentPaid, hireCost);
                    
                    if (currentPaid >= hireCost)
                    {
                        _CompleteHiring();
                        yield break;
                    }
                }
            }
            
            // 2. 지불 간격 조절
            yield return _waitDuration2;
        }
        
        _paymentCoroutine = null;
    }

    private IEnumerator _FlyToZone(GameObject money)
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

    private void _CompleteHiring()
    {
        if (prisonerGroup != null) prisonerGroup.SetActive(true);
        gameObject.SetActive(false); 
        Debug.Log("죄수 고용 완료!");
    }
}