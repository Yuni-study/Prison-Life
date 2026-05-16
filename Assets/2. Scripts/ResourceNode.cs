using UnityEngine;
using System.Collections;

public class ResourceNode : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private string resourceTag = "Coal"; 
    [SerializeField] private GameObject resourcePrefab;   // 캘 때 플레이어에게 줄 프리팹
    [SerializeField] private int maxHealth = 5;            // 한 노드에서 캘 수 있는 총 개수
    [SerializeField] private float respawnTime = 5f;       // 재생성 대기 시간

    [Header("참조")]
    [SerializeField] private GameObject visualModel;       // 사라지고 나타날 모델(Cube 등)
    
    private int currentHealth;
    private bool isMining = false;
    private bool isAvailable = true;
    private Collider nodeCollider;

    private void Start()
    {
        currentHealth = maxHealth;
        nodeCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어이고, 자원이 현재 있는 상태(Available)일 때만 채굴 시작
        if (isAvailable && other.CompareTag("Player"))
        {
            isMining = true;
            StartCoroutine(MiningRoutine(other.GetComponent<PlayerStacker>()));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isMining = false;
        }
    }

    IEnumerator MiningRoutine(PlayerStacker stacker)
    {
        // 플레이어가 영역 안에 있고, 노드에 자원이 남아있을 때 반복
        while (isMining && isAvailable && stacker != null)
        {
            // if (stacker.IsFull())
            // {
            //     stacker.ShowMaxText();
            //     yield return new WaitForSeconds(1f); // 꽉 찼을 땐 잠시 대기
            // }
            if (stacker.IsFull("Coal"))
            {
                stacker.ShowMaxText();
                yield return new WaitForSeconds(1f); // 꽉 찼을 땐 잠시 대기
            }
            else
            {
                // 1. 자원 생성 및 플레이어에게 전달
                GameObject newCoal = Instantiate(resourcePrefab);
                stacker.AddItemToList(newCoal, resourceTag);
                SoundManager.Instance.PlaySFX(SoundManager.Instance.mineClip);
                
                // 2. 체력(남은 양) 감소
                currentHealth--;

                // 3. 자원이 다 떨어졌다면 재생성 루틴 시작
                if (currentHealth <= 0)
                {
                    StartCoroutine(RespawnProcess());
                    yield break; // 코루틴 종료
                }
            }

            // UpgradeManager에서 현재 업그레이드된 속도를 가져와 반영
            yield return new WaitForSeconds(UpgradeManager.Instance.GetCurrentMineInterval());
        }
    }

    IEnumerator RespawnProcess()
    {
        isAvailable = false;
        isMining = false;

        // 시각적으로 사라지게 하고 콜라이더를 꺼서 채굴 방지
        visualModel.SetActive(false);
        nodeCollider.enabled = false;

        // 대기 시간
        yield return new WaitForSeconds(respawnTime);

        // 다시 초기화 및 활성화
        currentHealth = maxHealth;
        visualModel.SetActive(true);
        nodeCollider.enabled = true;
        isAvailable = true;
        
        // (선택) 재생성 시 파티클 효과를 실행하면 훨씬 생동감 넘칩니다.
    }

    public bool ExecuteMining() 
    {
        if (!isAvailable) return false;

        currentHealth--;

        // 자원이 다 떨어졌다면 재생성 루틴 시작
        if (currentHealth <= 0)
        {
            StartCoroutine(RespawnProcess());
        }
        
        return true; // 성공적으로 캤음
    }
}