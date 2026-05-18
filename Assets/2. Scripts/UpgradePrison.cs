using UnityEngine;
using System.Collections;
using Cinemachine;

public class UpgradePrison : MonoBehaviour
{
    public int upgradeCost = 5000;
    public int currentPaid = 0;
    public ZoneUI zoneUI;
    public GameObject victoryUI; // 승리 팝업
    public GameObject basicJailObject; // 기존 감옥 모델
    public GameObject prisonExpansionVisual; // 확장된 감옥 모델

    private bool _isPlayerInside = false;
    private PlayerStacker _playerStacker;

    [Header("Cameras")]
    public CinemachineVirtualCamera playerCam;  // 플레이어 카메라
    public CinemachineVirtualCamera prisonCam;  // 감옥 조망 카메라

    private bool _isUpgrading = false; // 중복 실행 방지

    private WaitForSeconds _waitDuration1;
    private WaitForSeconds _waitDuration2;
    private WaitForSeconds _waitDuration3;
    private WaitForSeconds _waitDuration4;

    private void Awake()
    {
        // // 0.05, 1.0, 2.0, 1.0
        _waitDuration1 = new WaitForSeconds(Constants.POINTONEFIVE);
        _waitDuration2 = new WaitForSeconds(Constants.ONE_FLOAT);
        _waitDuration3 = new WaitForSeconds(Constants.TWO_FLOAT);
        _waitDuration4 = new WaitForSeconds(Constants.ONE_FLOAT);
    }

    private void Start()
    {
        if(victoryUI != null) victoryUI.SetActive(false);
        if(zoneUI != null) zoneUI.UpdateUI(currentPaid, upgradeCost);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isUpgrading) return;
        
        if (other.CompareTag("Player"))
        {
            // 죄수 20명이 찼을 때만 돈을 받음
            if (PrisonerManager.Instance.GetCurrentPrisonerCount() >= Constants.TWENTY_INTEGER)
            {
                _isPlayerInside = true;
                _playerStacker = other.GetComponent<PlayerStacker>();
                StartCoroutine(_PaymentRoutine());
            }
            else
            {
                Debug.Log("죄수 20명을 먼저 감옥에 가두어야 합니다!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) _isPlayerInside = false;
    }

    private IEnumerator _PaymentRoutine()
    {
        while (_isPlayerInside && currentPaid < upgradeCost)
        {
            GameObject money = _playerStacker.PopSpecificItem("Money");
            if (money != null)
            {
                _playerStacker.AddMoney(-100); // 돈 하나당 100원으로 가정
                Destroy(money);
                currentPaid += 100; // 돈 하나당 가치
                if(zoneUI != null) zoneUI.UpdateUI("EXPAND PRISON", upgradeCost - currentPaid);
                yield return _waitDuration1; 
            }
            else yield return null;
        }

        if (currentPaid >= upgradeCost && !_isUpgrading)
        {
            StartCoroutine(_ExpansionSequence());
        }
    }

    private IEnumerator _ExpansionSequence()
    {
        _isUpgrading = true;
        
        // 1. 카메라를 감옥으로 전환
        prisonCam.Priority = Constants.TWENTY_INTEGER; // 플레이어 카메라(10)보다 높게 설정
        yield return _waitDuration2; // 카메라가 블렌딩되는 시간 대기

        // 2. 감옥 확장 연출 (모델 교체)
        if(basicJailObject != null) basicJailObject.SetActive(false);
        if(prisonExpansionVisual != null) prisonExpansionVisual.SetActive(true);
        
        // (선택) 여기서 파티클 효과나 효과음을 재생하면 더 좋습니다.
        yield return _waitDuration3; // 확장된 감옥을 잠시 감상

        // 3. 카메라를 다시 플레이어에게 복귀
        prisonCam.Priority = 5; 
        yield return _waitDuration4; // 다시 돌아오는 시간 대기

        // 4. 승리 UI 표시
        if(victoryUI != null) victoryUI.SetActive(true);

        // 발판 제거
        gameObject.SetActive(false);
    }
}