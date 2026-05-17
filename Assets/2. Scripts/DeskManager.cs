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