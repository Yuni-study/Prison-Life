using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("References")]
    public Renderer playerRenderer;      // 캐릭터 메쉬 렌더러
    public BoxCollider collectArea;   // 자원 수집용 트리거 콜라이더 (범위 넓히기용)
    public PlayerStacker stacker;

    [Header("Upgrade Settings")]
    public int upgradeLevel = 0; // 0: 기본, 1: 1단계(파랑), 2: 2단계(빨강)

    // 레벨별 색상 설정
    public Color[] levelColors = { Color.white, Color.blue, Color.red };

    // 레벨별 박스 콜라이더 사이즈 (X, Y, Z)
    public Vector3[] collectSizes = { 
        new Vector3(2f, 2f, 2f), 
        new Vector3(3.5f, 2f, 3.5f), 
        new Vector3(5f, 2f, 5f) 
    };

    [Header("Mining Settings")]
    public int mineLevel = 1;
    public float[] mineIntervals = { 0.5f, 0.4f, 0.3f, 0.2f, 0.1f }; // 레벨별 채굴 속도

    [Header("Capacity Settings")]
    public int capacityLevel = 1;
    public int[] maxCapacities = { 20, 30, 45, 60, 80 }; // 레벨별 최대 소지량

    [Header("Final Unlock")]
    public GameObject prisonUpgradeZone; // 감옥 업그레이드 발판 오브젝트

    void Awake()
    {
        Instance = this;
        // stacker = GetComponent<PlayerStacker>();
    }

    public void UpgradeMineSpeed()
    {
        if (mineLevel < mineIntervals.Length)
        {
            mineLevel++;
            Debug.Log($"채굴 업그레이드! 현재 레벨: {mineLevel}");

            CheckAllUpgradesMax();
        }
    }

    public void UpgradeCapacity()
    {
        if (capacityLevel < maxCapacities.Length)
        {
            capacityLevel++;
            stacker.maxCapacity = maxCapacities[capacityLevel - 1];
            Debug.Log($"소지량 업그레이드! 현재 최대치: {stacker.maxCapacity}");

            CheckAllUpgradesMax();
        }
    }

    // 업그레이드 실행 함수 (UpgradeZone에서 호출)
    public void ProcessUpgrade()
    {
        if (upgradeLevel < 2)
        {
            upgradeLevel++;
            ApplyUpgradeEffects();

            // [추가] 캐릭터 업그레이드가 끝났을 때도 최종 해금 조건인지 확인해야 합니다.
            CheckAllUpgradesMax();
        }
    }

    private void ApplyUpgradeEffects()
    {
        // 1. 소지량 10씩 증가
        stacker.maxCapacity += 10;

        // 2. 박스 콜라이더 범위 확장
        if (collectArea != null)
        {
            collectArea.size = collectSizes[upgradeLevel];
        }

        // 3. 캐릭터 색상 변경
        if (playerRenderer != null)
        {
            // 쉐이더 종류에 따라 "_Color" 혹은 "_BaseColor"
            playerRenderer.material.color = levelColors[upgradeLevel];
        }

        Debug.Log($"업그레이드 완료! 레벨: {upgradeLevel}, 소지량: {stacker.maxCapacity}, 범위(Size): {collectArea.size}");
    }

    // 현재 채굴 간격 (CoalSource에서 참조)
    public float GetCurrentMineInterval() 
    {
        // 기본 0.5초에서 레벨당 0.2초씩 단축
        return Mathf.Max(0.1f, 0.5f - (upgradeLevel * 0.2f));
    }

    public void CheckAllUpgradesMax()
    {
        // mineLevel이 5일 때 (배열 길이 5와 같음) MAX
        bool isMineMax = (mineLevel >= mineIntervals.Length);
        
        // capacityLevel이 5일 때 (배열 길이 5와 같음) MAX
        bool isCapMax = (capacityLevel >= maxCapacities.Length);
        
        // upgradeLevel이 2일 때 MAX
        bool isPlayerMax = (upgradeLevel >= 2); 

        // 디버깅을 위해 로그를 찍어보세요. 어느 것이 false인지 알 수 있습니다.
        Debug.Log($"상태 체크 - 광산: {isMineMax}, 가방: {isCapMax}, 캐릭터: {isPlayerMax}");

        if (isMineMax && isCapMax && isPlayerMax)
        {
            if (prisonUpgradeZone != null)
            {
                prisonUpgradeZone.SetActive(true); 
                Debug.Log("모든 인프라 업그레이드 완료! 최종 감옥 확장 가능.");
            }
            else
            {
                Debug.LogError("prisonUpgradeZone 오브젝트가 인스펙터에서 할당되지 않았습니다!");
            }
        }
    }
}