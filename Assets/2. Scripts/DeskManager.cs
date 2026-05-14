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

    [Header("Speed Settings")] // 속도 제어 변수 통합
    public float transferInterval = 0.03f; // 투입 간격 (0.2 -> 0.03) 매우 빠름
    public float moveDuration = 0.1f;     // 이동 시간 (0.2 -> 0.1) 
    public float stackHeightOffset = 0.2f; // 수갑 쌓이는 간격 (조금 촘촘하게)

    // 수갑 리스트 (현재 데스크에 있는 것들)
    public List<GameObject> handcuffsOnDesk = new List<GameObject>();
    private PlayerStacker currentPlayer;
    private bool isPlayerInDeliveryArea = false;
    private Prisoner targetPrisoner;
    private bool isServing = false; // 중복 실행 방지

    [Header("Money Stack")]
    public List<GameObject> moneyOnDesk = new List<GameObject>();
    public float moneyStackHeightOffset = 0.1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentPlayer = other.GetComponent<PlayerStacker>();
            isPlayerInDeliveryArea = true;
            // 중복 실행 방지를 위해 확실히 정지 후 실행
            StopCoroutine(nameof(TransferHandcuffsRoutine));
            StartCoroutine(nameof(TransferHandcuffsRoutine));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInDeliveryArea = false;
            currentPlayer = null;
        }
    }

    public void SetTargetPrisoner(Prisoner p)
    {
        targetPrisoner = p;
        if (!isServing) StartCoroutine(ServeRoutine());
    }

    IEnumerator ServeRoutine()
    {
        isServing = true;
        int count = 0;

        while (targetPrisoner != null && count < targetPrisoner.needHandcuffs)
        {
            if (handcuffsOnDesk.Count > 0)
            {
                GameObject handcuff = handcuffsOnDesk[0];
                handcuffsOnDesk.RemoveAt(0);

                // 죄수에게 주는 속도 (필요시 단축)
                yield return new WaitForSeconds(0.1f); 

                if (handcuff != null)
                {
                    Destroy(handcuff);
                    count++;
                    ReorderDeskStack();
                }
                yield return new WaitForSeconds(0.1f);
            }
            yield return null;
        }

        if (targetPrisoner != null)
        {
            yield return new WaitForSeconds(0.2f);
            SpawnMoney();
            prisonerManager.DismissPrisoner();
            targetPrisoner = null;
        }
        isServing = false;
    }

    void SpawnMoney()
    {
        if (moneyPrefab != null && moneyPoint != null)
        {
            GameObject money = Instantiate(moneyPrefab, moneyPoint);
            float newY = moneyOnDesk.Count * moneyStackHeightOffset;
            money.transform.localPosition = new Vector3(0, newY, 0);
            money.transform.localRotation = Quaternion.identity;
            moneyOnDesk.Add(money);
            money.tag = "Money"; 
        }
    }

    void ReorderDeskStack()
    {
        for (int i = 0; i < handcuffsOnDesk.Count; i++)
        {
            if (handcuffsOnDesk[i] != null)
            {
                float newY = i * stackHeightOffset;
                // 재정렬도 즉시 이동하도록 수정
                handcuffsOnDesk[i].transform.localPosition = new Vector3(0, newY, 0);
            }
        }
    }

    IEnumerator TransferHandcuffsRoutine()
    {
        while (isPlayerInDeliveryArea && currentPlayer != null)
        {
            GameObject handcuff = currentPlayer.PopSpecificItem("Handcuff");
            if (handcuff != null)
            {
                handcuffsOnDesk.Add(handcuff);
                handcuff.transform.SetParent(deskStackPoint);

                float newY = (handcuffsOnDesk.Count - 1) * stackHeightOffset;
                Vector3 targetLocalPos = new Vector3(0, newY, 0);

                // 부드러운 이동 실행 (빨라진 duration 적용)
                StartCoroutine(MoveToDesk(handcuff, targetLocalPos));
                
                // 다음 수갑을 뽑는 대기 시간을 최소화
                yield return new WaitForSeconds(transferInterval);
            }
            else
            {
                // 수갑이 없으면 매 프레임 체크하며 대기
                yield return null;
            }
        }
    }

    IEnumerator MoveToDesk(GameObject obj, Vector3 targetPos)
    {
        float elapsed = 0f;
        Vector3 startPos = obj.transform.localPosition;
        Quaternion startRot = obj.transform.localRotation;

        while (elapsed < moveDuration)
        {
            if (obj == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;

            obj.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            obj.transform.localRotation = Quaternion.Lerp(startRot, Quaternion.identity, t);
            yield return null;
        }
        if (obj != null)
        {
            obj.transform.localPosition = targetPos;
            obj.transform.localRotation = Quaternion.identity;
        }
    }
}