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

    private bool isPlayerInside = false;
    private PlayerStacker playerStacker;

    [Header("Cameras")]
    public CinemachineVirtualCamera playerCam;  // 플레이어 카메라
    public CinemachineVirtualCamera prisonCam;  // 감옥 조망 카메라

    private bool isUpgrading = false; // 중복 실행 방지

    void Start()
    {
        if(victoryUI != null) victoryUI.SetActive(false);
        if(zoneUI != null) zoneUI.UpdateUI(currentPaid, upgradeCost);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isUpgrading) return;
        
        if (other.CompareTag("Player"))
        {
            // 죄수 20명이 찼을 때만 돈을 받음
            if (PrisonerManager.Instance.GetCurrentPrisonerCount() >= 20)
            {
                isPlayerInside = true;
                playerStacker = other.GetComponent<PlayerStacker>();
                StartCoroutine(PaymentRoutine());
            }
            else
            {
                Debug.Log("죄수 20명을 먼저 감옥에 가두어야 합니다!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerInside = false;
    }

    // IEnumerator PaymentRoutine()
    // {
    //     while (isPlayerInside && currentPaid < upgradeCost)
    //     {
    //         GameObject money = playerStacker.PopSpecificItem("Money");
    //         if (money != null)
    //         {
    //             // FlyToZone 연출 (기존 UpgradeZone 코드 활용)
    //             currentPaid += 100;
    //             zoneUI.UpdateUI(currentPaid, upgradeCost);
    //             Destroy(money); 

    //             if (currentPaid >= upgradeCost)
    //             {
    //                 CompleteFinalUpgrade();
    //                 yield break;
    //             }
    //         }
    //         yield return new WaitForSeconds(0.1f);
    //     }
    // }
    IEnumerator PaymentRoutine()
    {
        while (isPlayerInside && currentPaid < upgradeCost)
        {
            GameObject money = playerStacker.PopSpecificItem("Money");
            if (money != null)
            {
                playerStacker.AddMoney(-100); // 돈 하나당 100원으로 가정
                Destroy(money);
                currentPaid += 100; // 돈 하나당 가치
                if(zoneUI != null) zoneUI.UpdateUI("EXPAND PRISON", upgradeCost - currentPaid);
                yield return new WaitForSeconds(0.05f);
            }
            else yield return null;
        }

        // if (currentPaid >= upgradeCost)
        // {
        //     CompleteFinalUpgrade();
        // }
        if (currentPaid >= upgradeCost && !isUpgrading)
        {
            StartCoroutine(ExpansionSequence());
        }
    }

    // void CompleteFinalUpgrade()
    // {
    //     // CameraDirector를 사용하여 감옥을 비춤
    //     CameraDirector.Instance.ShowUpgradeArea(() => {
    //         // 연출 종료 후 승리 UI 띄우기
    //         if(victoryUI != null) victoryUI.SetActive(true);
    //     });

    //     // 연출 도중 감옥 외형 변경
    //     if(prisonExpansionVisual != null) prisonExpansionVisual.SetActive(true);
    //     gameObject.SetActive(false); // 업그레이드 존 삭제
    // }
    // void CompleteFinalUpgrade()
    // {
    //     // 1. 시각적 변화
    //     prisonExpansionVisual.SetActive(true);
        
    //     // 2. PrisonerManager에 최종 업그레이드 완료 상태 전달
    //     PrisonerManager.Instance.isFinalUpgradeComplete = true;

    //     // 3. 승리 연출
    //     CameraDirector.Instance.ShowUpgradeArea(() => {
    //         victoryUI.SetActive(true);
    //     });

    //     gameObject.SetActive(false); // 업그레이드 존 삭제
    // }
    void CompleteFinalUpgrade()
    {
        // 1. 기존 감옥 끄고 새로운 감옥 켜기
        if(basicJailObject != null) basicJailObject.SetActive(false);
        if(prisonExpansionVisual != null) prisonExpansionVisual.SetActive(true);

        // 2. 승리 UI 띄우기
        if(victoryUI != null) victoryUI.SetActive(true);

        // 3. 발판 제거
        gameObject.SetActive(false);
        
        Debug.Log("게임 클리어! 감옥이 확장되었습니다.");
    }

    IEnumerator ExpansionSequence()
    {
        isUpgrading = true;
        
        // 1. 카메라를 감옥으로 전환
        prisonCam.Priority = 20; // 플레이어 카메라(10)보다 높게 설정
        yield return new WaitForSeconds(1.0f); // 카메라가 블렌딩되는 시간 대기

        // 2. 감옥 확장 연출 (모델 교체)
        if(basicJailObject != null) basicJailObject.SetActive(false);
        if(prisonExpansionVisual != null) prisonExpansionVisual.SetActive(true);
        
        // (선택) 여기서 파티클 효과나 효과음을 재생하면 더 좋습니다.
        yield return new WaitForSeconds(2.0f); // 확장된 감옥을 잠시 감상

        // 3. 카메라를 다시 플레이어에게 복귀
        prisonCam.Priority = 5; 
        yield return new WaitForSeconds(1.0f); // 다시 돌아오는 시간 대기

        // 4. 승리 UI 표시
        if(victoryUI != null) victoryUI.SetActive(true);

        // 발판 제거
        gameObject.SetActive(false);
    }
}