using UnityEngine;

public class StaffUpgradeZone : MonoBehaviour
{
    public int unlockCost = 1000;
    public GameObject staffPrefab;
    public Transform spawnPoint;
    public ZoneUI zoneUI;

    [Header("Targets for Staff")]
    public Transform targetOutput; // 인스펙터에서 ResourceConverter의 OutputPoint 연결
    public Transform targetDesk;   // 인스펙터에서 DeskManager 연결

    private int currentPaid = 0;
    private bool isUnlocked = false;

    [Header("Payment Speed")]
    public float payInterval = 0.05f; // 돈이 나가는 속도
    private float lastPayTime;

    private void OnTriggerStay(Collider other)
    {
        if (isUnlocked) return;

        if (other.CompareTag("Player"))
        {
            PlayerStacker player = other.GetComponent<PlayerStacker>();
            
            // 결제 주기 확인 (너무 매 프레임 깎이지 않게)
            if (Time.time >= lastPayTime + payInterval)
            {
                // 1. 플레이어가 돈을 가지고 있는지 확인 (수치상)
                if (player.CurrentMoney > 0 && currentPaid < unlockCost)
                {
                    // 2. 플레이어의 등에서 실제 돈 오브젝트 하나 추출
                    GameObject moneyObj = player.PopSpecificItem("Money");
                    
                    if (moneyObj != null)
                    {
                        Destroy(moneyObj); // 오브젝트 파괴
                        
                        // 3. 금액 처리 (돈 하나당 가치 설정, 예: 100원)
                        int moneyValue = 100; 
                        currentPaid += moneyValue;
                        player.CurrentMoney -= moneyValue; // 프로퍼티를 통해 UI 자동 갱신

                        // 4. UI 업데이트
                        if (zoneUI != null)
                            zoneUI.UpdateUI("HIRE STAFF", unlockCost - currentPaid);

                        lastPayTime = Time.time;
                        
                        // 효과음 (선택 사항)
                        // SoundManager.Instance.PlaySFX(SoundManager.Instance.inputClip);
                    }
                }

                // 5. 결제 완료 체크
                if (currentPaid >= unlockCost)
                {
                    UnlockStaff();
                }
            }
        }
    }

    // void UnlockStaff()
    // {
    //     // 1. 직원 생성
    //     GameObject newStaff = Instantiate(staffPrefab, spawnPoint.position, Quaternion.identity);
        
    //     // 2. 직원 스크립트 가져오기
    //     StaffAI ai = newStaff.GetComponent<StaffAI>();
        
    //     // 3. 씬의 오브젝트 정보 넘겨주기
    //     if (ai != null)
    //     {
    //         ai.outputArea = targetOutput;
    //         ai.deskArea = targetDesk;
    //     }

    //     gameObject.SetActive(false);
    // }
    void UnlockStaff()
    {
        isUnlocked = true;
        GameObject newStaff = Instantiate(staffPrefab, spawnPoint.position, Quaternion.identity);
        
        // 직원에게 목표 지점 전달
        StaffAI ai = newStaff.GetComponent<StaffAI>();
        if (ai != null)
        {
            ai.outputArea = targetOutput;
            ai.deskArea = targetDesk;
        }

        if (zoneUI != null) zoneUI.UpdateUI("COMPLETED", 0);
        gameObject.SetActive(false); // 발판 제거
    }
}