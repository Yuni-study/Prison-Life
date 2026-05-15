// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;

// public class DeskManager : MonoBehaviour
// {
//     [Header("References")]
//     public PrisonerManager prisonerManager;
//     public Transform deskStackPoint;
//     public Transform moneyPoint; 
//     public GameObject moneyPrefab;

//     [Header("Speed Settings")] // 속도 제어 변수 통합
//     public float transferInterval = 0.03f; // 투입 간격 (0.2 -> 0.03) 매우 빠름
//     public float moveDuration = 0.1f;     // 이동 시간 (0.2 -> 0.1) 
//     public float stackHeightOffset = 0.2f; // 수갑 쌓이는 간격 (조금 촘촘하게)

//     // 수갑 리스트 (현재 데스크에 있는 것들)
//     public List<GameObject> handcuffsOnDesk = new List<GameObject>();
//     private PlayerStacker currentPlayer;
//     private bool isPlayerInDeliveryArea = false;
//     private Prisoner targetPrisoner;
//     private bool isServing = false; // 중복 실행 방지

//     [Header("Money Stack")]
//     public List<GameObject> moneyOnDesk = new List<GameObject>();
//     public float moneyStackHeightOffset = 0.1f;

//     [Header("Multi-User Support")]
//     // 영역 안에 있는 모든 Stacker를 관리하는 리스트
//     private List<PlayerStacker> stackersInRange = new List<PlayerStacker>();

//     private void OnTriggerEnter(Collider other)
//     {
//         // 플레이어나 스태프가 들어오면
//         if (other.CompareTag("Player") || other.CompareTag("Staff"))
//         {
//             // 1. 들어온 대상의 PlayerStacker를 가져와 현재 작업자로 설정
//             PlayerStacker targetStacker = other.GetComponent<PlayerStacker>();
            
//             if (targetStacker != null && stackersInRange.Contains(targetStacker))
//             {
//                 stackersInRange.Add(targetStacker);

//                 // currentPlayer = targetStacker;
//                 // isPlayerInDeliveryArea = true;

//                 // // 2. 기존 루틴이 있다면 멈추고 새로 시작 (아이템 전달 시작)
//                 // StopCoroutine(nameof(TransferHandcuffsRoutine));
//                 // StartCoroutine(nameof(TransferHandcuffsRoutine));
                
//                 // Debug.Log($"{other.tag}가 데스크에 도착했습니다. 수갑 전달을 시작합니다.");

//                 // 루틴이 실행 중이 아니라면 시작
//                 if (!isServing) StartCoroutine(nameof(ServeRoutine));
                
//                 // 수갑 전달 루틴도 실행 중이 아니라면 시작
//                 StopCoroutine(nameof(TransferHandcuffsRoutine));
//                 StartCoroutine(nameof(TransferHandcuffsRoutine));
//             }
//         }
//     }

//     // private void OnTriggerExit(Collider other)
//     // {
//     //     if (other.CompareTag("Player") || other.CompareTag("Staff"))
//     //     {
//     //         // 나가는 대상이 현재 서비스 중인 대상일 때만 중지
//     //         if (currentPlayer == other.GetComponent<PlayerStacker>())
//     //         {
//     //             isPlayerInDeliveryArea = false;
//     //             currentPlayer = null;
//     //             Debug.Log($"{other.tag}가 영역을 벗어났습니다.");
//     //         }
//     //     }
//     // }
//     private void OnTriggerExit(Collider other)
//     {
//         if (other.CompareTag("Player") || other.CompareTag("Staff"))
//         {
//             PlayerStacker ps = other.GetComponent<PlayerStacker>();
//             if (ps != null && stackersInRange.Contains(ps))
//             {
//                 stackersInRange.Remove(ps);
//             }
//         }
//     }

//     public void SetTargetPrisoner(Prisoner p)
//     {
//         targetPrisoner = p;
//         if (!isServing) StartCoroutine(ServeRoutine());
//     }

//     IEnumerator ServeRoutine()
//     {
//         // Instance가 비어있는지, 혹은 PrisonFull 체크에서 문제가 없는지 확인
//         if (PrisonerManager.Instance == null)
//         {
//             Debug.LogError("PrisonerManager Instance를 찾을 수 없습니다!");
//             yield break;
//         }
    
//         // [추가] 감옥 수용량 확인
//         if (PrisonerManager.Instance.IsPrisonFull())
//         {
//             Debug.Log("감옥이 가득 차서 더 이상 죄수를 받을 수 없습니다!");
//             isServing = false;
//             yield break; 
//         }

//         isServing = true;
//         int count = 0;

//         while (targetPrisoner != null && count < targetPrisoner.needHandcuffs)
//         {
//             if (handcuffsOnDesk.Count > 0)
//             {
//                 GameObject handcuff = handcuffsOnDesk[0];
//                 handcuffsOnDesk.RemoveAt(0);
//                 SoundManager.Instance.PlaySFX(SoundManager.Instance.prisonerGetClip);

//                 // 죄수에게 주는 속도 (필요시 단축)
//                 yield return new WaitForSeconds(0.1f); 

//                 if (handcuff != null)
//                 {
//                     Destroy(handcuff);
//                     count++;
//                     ReorderDeskStack();
//                 }
//                 yield return new WaitForSeconds(0.1f);
//             }
//             yield return null;
//         }

//         if (targetPrisoner != null)
//         {
//             yield return new WaitForSeconds(0.2f);
//             SoundManager.Instance.PlaySFX(SoundManager.Instance.moneySpawnClip);
//             SpawnMoney();
//             prisonerManager.DismissPrisoner();
//             targetPrisoner = null;
//         }
//         isServing = false;
//     }

//     void SpawnMoney()
//     {
//         if (moneyPrefab != null && moneyPoint != null)
//         {
//             GameObject money = Instantiate(moneyPrefab, moneyPoint);
//             float newY = moneyOnDesk.Count * moneyStackHeightOffset;
//             money.transform.localPosition = new Vector3(0, newY, 0);
//             money.transform.localRotation = Quaternion.identity;
//             moneyOnDesk.Add(money);
//             money.tag = "Money"; 
//         }
//     }

//     void ReorderDeskStack()
//     {
//         for (int i = 0; i < handcuffsOnDesk.Count; i++)
//         {
//             if (handcuffsOnDesk[i] != null)
//             {
//                 float newY = i * stackHeightOffset;
//                 // 재정렬도 즉시 이동하도록 수정
//                 handcuffsOnDesk[i].transform.localPosition = new Vector3(0, newY, 0);
//             }
//         }
//     }

//     // IEnumerator TransferHandcuffsRoutine()
//     // {
//     //     while (isPlayerInDeliveryArea && currentPlayer != null)
//     //     {
//     //         GameObject handcuff = currentPlayer.PopSpecificItem("Handcuff");
//     //         if (handcuff != null)
//     //         {
//     //             handcuffsOnDesk.Add(handcuff);
//     //             handcuff.transform.SetParent(deskStackPoint);

//     //             float newY = (handcuffsOnDesk.Count - 1) * stackHeightOffset;
//     //             Vector3 targetLocalPos = new Vector3(0, newY, 0);

//     //             // 부드러운 이동 실행 (빨라진 duration 적용)
//     //             StartCoroutine(MoveToDesk(handcuff, targetLocalPos));
                
//     //             // 다음 수갑을 뽑는 대기 시간을 최소화
//     //             yield return new WaitForSeconds(transferInterval);
//     //         }
//     //         else
//     //         {
//     //             // 수갑이 없으면 매 프레임 체크하며 대기
//     //             yield return null;
//     //         }
//     //     }
//     // }
//     IEnumerator TransferHandcuffsRoutine()
//     {
//         // 영역 안에 누군가 있는 동안 계속 반복
//         while (stackersInRange.Count > 0)
//         {
//             bool movedAny = false;

//             // 영역 안의 모든 사람을 체크
//             for (int i = 0; i < stackersInRange.Count; i++)
//             {
//                 PlayerStacker target = stackersInRange[i];
//                 if (target == null) continue;

//                 // 해당 타겟에게 수갑이 있다면 하나 가져옴
//                 GameObject handcuff = target.PopSpecificItem("Handcuff");
//                 if (handcuff != null)
//                 {
//                     handcuffsOnDesk.Add(handcuff);
//                     handcuff.transform.SetParent(deskStackPoint);

//                     float newY = (handcuffsOnDesk.Count - 1) * stackHeightOffset;
//                     Vector3 targetLocalPos = new Vector3(0, newY, 0);

//                     StartCoroutine(MoveToDesk(handcuff, targetLocalPos));
                    
//                     movedAny = true;
//                     yield return new WaitForSeconds(transferInterval);
//                     break; // 한 번에 하나씩만 처리하고 다음 루프로
//                 }
//             }

//             // 이번 루프에서 아무도 수갑을 주지 않았다면 잠시 대기
//             if (!movedAny) yield return null;
//         }
//     }

//     IEnumerator MoveToDesk(GameObject obj, Vector3 targetPos)
//     {
//         float elapsed = 0f;
//         Vector3 startPos = obj.transform.localPosition;
//         Quaternion startRot = obj.transform.localRotation;

//         while (elapsed < moveDuration)
//         {
//             if (obj == null) yield break;
//             elapsed += Time.deltaTime;
//             float t = elapsed / moveDuration;

//             obj.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
//             obj.transform.localRotation = Quaternion.Lerp(startRot, Quaternion.identity, t);
//             yield return null;
//         }
//         if (obj != null)
//         {
//             obj.transform.localPosition = targetPos;
//             obj.transform.localRotation = Quaternion.identity;
//         }
//     }
// }

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeskManager : MonoBehaviour
{
    [Header("References")]
    public PrisonerManager prisonerManager;
    public Transform deskStackPoint;
    public Transform moneyPoint; 
    public GameObject moneyPrefab;

    [Header("Settings")]
    public float transferInterval = 0.05f;
    public float stackHeightOffset = 0.2f;
    public float moneyStackHeightOffset = 0.1f;

    [Header("State")]
    public List<GameObject> handcuffsOnDesk = new List<GameObject>();
    public List<GameObject> moneyOnDesk = new List<GameObject>();
    private List<PlayerStacker> stackersInRange = new List<PlayerStacker>();
    
    private Prisoner targetPrisoner;
    private bool isServing = false; // 죄수에게 수갑을 주는 루틴 중복 방지
    private bool isTransferring = false; // 캐릭터에게서 수갑을 뺏어오는 루틴 중복 방지

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Staff"))
        {
            PlayerStacker ps = other.GetComponent<PlayerStacker>();
            if (ps != null && !stackersInRange.Contains(ps))
            {
                stackersInRange.Add(ps);
                
                // 캐릭터가 들어오면 수갑 수거 루틴 시작
                if (!isTransferring) StartCoroutine(TransferHandcuffsRoutine());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Staff"))
        {
            PlayerStacker ps = other.GetComponent<PlayerStacker>();
            if (ps != null) stackersInRange.Remove(ps);
        }
    }

    // --- 수거 루틴: 캐릭터(플레이어/직원)에게서 수갑을 데스크로 가져옴 ---
    IEnumerator TransferHandcuffsRoutine()
    {
        isTransferring = true;
        while (stackersInRange.Count > 0)
        {
            bool foundItem = false;
            foreach (var stacker in stackersInRange)
            {
                if (stacker == null) continue;
                
                GameObject handcuff = stacker.PopSpecificItem("Handcuff");
                if (handcuff != null)
                {
                    handcuffsOnDesk.Add(handcuff);
                    handcuff.transform.SetParent(deskStackPoint);
                    
                    float newY = (handcuffsOnDesk.Count - 1) * stackHeightOffset;
                    StartCoroutine(MoveToDesk(handcuff, new Vector3(0, newY, 0)));
                    
                    foundItem = true;
                    yield return new WaitForSeconds(transferInterval);
                    break; 
                }
            }
            if (!foundItem) yield return null;
        }
        isTransferring = false;
    }

    // --- 서비스 루틴: 데스크에 있는 수갑을 죄수에게 전달 ---
    public void SetTargetPrisoner(Prisoner p)
    {
        targetPrisoner = p;
        if (!isServing) StartCoroutine(ServeRoutine());
    }

    IEnumerator ServeRoutine()
    {
        isServing = true;
        while (targetPrisoner != null)
        {
            if (handcuffsOnDesk.Count > 0)
            {
                GameObject handcuff = handcuffsOnDesk[0];
                handcuffsOnDesk.RemoveAt(0);
                
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlaySFX(SoundManager.Instance.prisonerGetClip);

                Destroy(handcuff);
                targetPrisoner.needHandcuffs--; // 죄수의 필요 수치 감소
                ReorderDeskStack();

                if (targetPrisoner.needHandcuffs <= 0)
                {
                    yield return new WaitForSeconds(0.2f);
                    SpawnMoney();
                    prisonerManager.DismissPrisoner();
                    targetPrisoner = null;
                }
                yield return new WaitForSeconds(0.15f);
            }
            else yield return null;
        }
        isServing = false;
    }

    // --- 공용 유틸리티 함수 ---
    void SpawnMoney()
    {
        if (moneyPrefab == null) return;
        GameObject money = Instantiate(moneyPrefab, moneyPoint);
        money.transform.localPosition = new Vector3(0, moneyOnDesk.Count * moneyStackHeightOffset, 0);
        moneyOnDesk.Add(money);
        money.tag = "Money";
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.moneySpawnClip);
    }

    void ReorderDeskStack()
    {
        for (int i = 0; i < handcuffsOnDesk.Count; i++)
        {
            if (handcuffsOnDesk[i] != null)
                handcuffsOnDesk[i].transform.localPosition = new Vector3(0, i * stackHeightOffset, 0);
        }
    }

    IEnumerator MoveToDesk(GameObject obj, Vector3 targetPos)
    {
        float elapsed = 0f;
        Vector3 startPos = obj.transform.localPosition;
        while (elapsed < 0.1f)
        {
            if (obj == null) yield break;
            elapsed += Time.deltaTime;
            obj.transform.localPosition = Vector3.Lerp(startPos, targetPos, elapsed / 0.1f);
            yield return null;
        }
        if (obj != null) obj.transform.localPosition = targetPos;
    }
}