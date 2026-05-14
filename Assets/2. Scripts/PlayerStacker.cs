using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerStacker : MonoBehaviour
{
    [Header("Anchors")]
    public Transform coalAnchor;
    public Transform moneyAnchor;
    public Transform handcuffAnchor;

    [Header("Settings")]
    public float verticalOffset = 0.25f; // 위로 쌓이는 간격
    public float horizontalOffset = 0.6f; // 구역 간의 최소 간격
    public int maxCapacity = 20;        // 최대 소지량

    // 종류별 리스트 관리
    private List<GameObject> coalList = new List<GameObject>();
    private List<GameObject> moneyList = new List<GameObject>();
    private List<GameObject> handcuffList = new List<GameObject>();

    [Header("Stack Settings")]
    public Transform stackPoint;        // 석탄이 쌓이기 시작할 위치
    public float stackHeightOffset = 0.25f; // 석탄 사이의 간격

    [Header("UI")]
    public GameObject maxTextUI;        // MAX 텍스트 오브젝트

    private Coroutine collectionCoroutine;

    // private List<GameObject> collectedObjects = new List<GameObject>();

    private int TotalCount => coalList.Count + moneyList.Count + handcuffList.Count;

    [Header("Economy")]
    public int currentMoney = 0;

    public bool IsFull() => TotalCount >= maxCapacity;

    public void AddItemToList(GameObject obj, string tag)
    {
        if (IsFull())
        {
            ShowMaxText();
            if (obj != null && obj.scene.name == null) Destroy(obj); // 새로 생성한거면 파괴
            return;
        }

        if (obj.TryGetComponent<Collider>(out Collider col)) col.enabled = false;

        switch (tag)
        {
            case "Coal": 
                coalList.Add(obj); 
                obj.transform.SetParent(coalAnchor);
                break;
            case "Money": 
                moneyList.Add(obj); 
                obj.transform.SetParent(moneyAnchor);
                break;
            case "Handcuff": 
                handcuffList.Add(obj); 
                obj.transform.SetParent(handcuffAnchor);
                break;
        }

        UpdateAllLayouts();
    }

    public void ShowMaxText() 
    { 
        if(!maxTextUI.activeSelf) 
        { 
            maxTextUI.SetActive(true);
            Invoke("HideMaxText", 1f);
            
        } 
    }

    private void HideMaxText()
    {
        maxTextUI.SetActive(false);
    }

    // 자원이 있는지 확인
    public bool HasResource() 
    {
        return TotalCount > 0;
    }

    // 석탄을 하나 버림 (변환기에 줄 때)
    // public void RemoveResource()
    // {
    //     if (collectedObjects.Count > 0)
    //     {
    //         int lastIndex = collectedObjects.Count - 1;
    //         GameObject toRemove = collectedObjects[lastIndex];
            
    //         // 리스트에서 먼저 제거하여 중복 참조 방지
    //         collectedObjects.RemoveAt(lastIndex);

    //         // 오브젝트가 존재할 때만 파괴
    //         if (toRemove != null)
    //         {
    //             Destroy(toRemove);
    //         }
    //     }
    // }

    // 자원 추가 (석탄/수갑 공용)
    // public bool AddSpecificResource(GameObject resourceObj)
    // {
    //     if (collectedObjects.Count >= maxCapacity) return false;

    //     collectedObjects.Add(resourceObj);
    //     resourceObj.transform.SetParent(stackPoint);

    //     // 중요: Lerp 이동 전에 IsTrigger를 끄거나 레이어를 변경하여 
    //     // 자기 자신의 수집 영역에 다시 닿지 않게 해야 합니다.
    //     if(resourceObj.TryGetComponent<Collider>(out Collider col)) col.enabled = false;

    //     float newY = (collectedObjects.Count - 1) * stackHeightOffset;
    //     Vector3 targetLocalPos = new Vector3(0, newY, 0);

    //     StartCoroutine(SmoothMoveToStack(resourceObj, targetLocalPos));
    //     return true;
    // }
    
    IEnumerator SmoothMoveToStack(GameObject obj, Vector3 targetPos)
    {
        if (obj == null) yield break;
        float elapsed = 0f;
        float duration = 0.2f;
        Vector3 startPos = obj.transform.localPosition;

        while (elapsed < duration)
        {
            if (obj == null) yield break;
            elapsed += Time.deltaTime;
            obj.transform.localPosition = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            obj.transform.localRotation = Quaternion.Lerp(obj.transform.localRotation, Quaternion.identity, elapsed / duration);
            yield return null;
        }
        if (obj != null) obj.transform.localPosition = targetPos;
    }

    // 석탄만 가지고 있는지 확인하는 함수 (변환기용)
    public bool HasCoal()
    {
        return coalList.Count > 0;
    }

    // 석탄만 제거하는 함수 (변환기용)
    public void RemoveCoal() 
    {
        GameObject item = PopSpecificItem("Coal");
        if (item != null) Destroy(item);
    }

    // 중간 자원이 빠졌을 때 나머지 자원들을 아래로 밀착시키는 기능
    // public void ReorderStack()
    // {
    //     for (int i = 0; i < collectedObjects.Count; i++)
    //     {
    //         float newY = i * stackHeightOffset;
    //         Vector3 targetPos = new Vector3(0, newY, 0);
    //         // 모든 아이템을 새로운 위치로 부드럽게 이동
    //         StartCoroutine(SmoothMoveToStack(collectedObjects[i], targetPos));
    //     }
    // }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Money"))
        {
            other.GetComponentInParent<DeskManager>()?.moneyOnDesk.Remove(other.gameObject);
            AddItemToList(other.gameObject, "Money");
        }
        else if (other.CompareTag("OutputArea"))
        {
            ResourceConverter converter = other.GetComponentInParent<ResourceConverter>();
            if (converter != null)
            {
                if (collectionCoroutine != null) StopCoroutine(collectionCoroutine);
                collectionCoroutine = StartCoroutine(CollectRoutine(converter));
            }
        }
    }

    // CollectRoutine의 대기 시간도 단축 (수갑 수집 속도)
    // IEnumerator CollectRoutine(ResourceConverter converter)
    // {
    //     while (!IsFull())
    //     {
    //         GameObject item = converter.TakeHandcuff();
    //         if (item != null)
    //         {
    //             AddItemToList(item, "Handcuff");
    //             yield return new WaitForSeconds(0.05f); // 0.15f -> 0.05f 상향
    //         }
    //         else break;
    //     }
    // }
    IEnumerator CollectRoutine(ResourceConverter converter)
    {
        while (!IsFull())
        {
            GameObject item = converter.TakeHandcuff();
            if (item != null)
            {
                AddItemToList(item, "Handcuff");
                // 가져오는 속도도 0.05초로 매우 빠르게 변경
                yield return new WaitForSeconds(0.05f); 
            }
            else
            {
                // 변환기에 수갑이 없으면 짧게 대기 후 다시 확인
                yield return new WaitForSeconds(0.1f);
                // if (!converter.HasHandcuffsInOutput()) break; // 체크용 함수 추가 시
            }
        }
    }

    // // 아이템을 리스트에 넣고 구역 배치를 다시 하는 통합 함수
    // public void AddItemToList(GameObject obj, string tag)
    // {
    //     if (GetCurrentTotalCount() >= maxCapacity) return;

    //     if (obj.TryGetComponent<Collider>(out Collider col)) col.enabled = false;

    //     // 태그에 맞는 리스트에 추가
    //     switch (tag)
    //     {
    //         case "Coal": 
    //             coalList.Add(obj); 
    //             obj.transform.SetParent(coalAnchor); // 즉시 부모 설정
    //             break;
    //         case "Money": 
    //             moneyList.Add(obj); 
    //             obj.transform.SetParent(moneyAnchor); // 즉시 부모 설정
    //             break;
    //         case "Handcuff": 
    //             handcuffList.Add(obj); 
    //             obj.transform.SetParent(handcuffAnchor); // 즉시 부모 설정
    //             break;
    //     }

    //     // 부모가 바뀌었으므로 로컬 좌표를 일단 0으로 초기화해서 튐 방지
    //     obj.transform.localPosition = Vector3.zero;

    //     // 전체 정렬 실행 (여기서 정확한 Y값과 Anchor 위치가 잡힙니다)
    //     UpdateAllLayouts();
    // }

    private int GetCurrentTotalCount()
    {
        return coalList.Count + moneyList.Count + handcuffList.Count;
    }

    // 핵심: 모든 구역의 위치와 아이템들의 위치를 재정렬
    public void UpdateAllLayouts()
    {
        // // 1. 석탄 (맨 앞)
        // SortZone(coalList, coalAnchor, Vector3.zero);

        // // 2. 돈 (석탄 뒤)
        // float moneyZ = (coalList.Count > 0) ? -horizontalOffset : 0f;
        // SortZone(moneyList, moneyAnchor, new Vector3(0, 0, moneyZ));

        // // 3. 수갑 (석탄+돈 뒤)
        // float handcuffZ = 0f;
        // if (coalList.Count > 0) handcuffZ -= horizontalOffset;
        // if (moneyList.Count > 0) handcuffZ -= horizontalOffset;
        // SortZone(handcuffList, handcuffAnchor, new Vector3(0, 0, handcuffZ));

        // 1. 석탄 (중앙)
        SortZone(coalList, coalAnchor, Vector3.zero);

        // 2. 돈 (왼쪽 열)
        // horizontalOffset 만큼 왼쪽(-X)으로 배치
        Vector3 moneyPos = new Vector3(-horizontalOffset, 0, 0);
        SortZone(moneyList, moneyAnchor, moneyPos);

        // 3. 수갑 (오른쪽 열)
        // horizontalOffset 만큼 오른쪽(+X)으로 배치
        Vector3 handcuffPos = new Vector3(horizontalOffset, 0, 0);
        SortZone(handcuffList, handcuffAnchor, handcuffPos);
    }

    // 특정 구역 내의 아이템들을 수직으로 정렬하고 구역 앵커를 이동
    private void SortZone(List<GameObject> list, Transform anchor, Vector3 anchorLocalPos)
    {
        if (anchor == null) return;
    
        // 구역 앵커의 로컬 위치 설정 (좌/우 분리)
        anchor.localPosition = anchorLocalPos;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null) continue;
            
            list[i].transform.SetParent(anchor);
            Vector3 targetPos = new Vector3(0, i * verticalOffset, 0);
            
            // 정렬 속도를 0.1f로 매우 빠르게 수정
            StartCoroutine(SmoothMove(list[i], targetPos, 0.1f));
        }
    }

    // 아이템 소모 시 호출 (예: 변환기에서 석탄 가져갈 때)
    public GameObject PopItem(string tag)
    {
        List<GameObject> targetList = null;
        if (tag == "Coal") targetList = coalList;
        else if (tag == "Money") targetList = moneyList;
        else if (tag == "Handcuff") targetList = handcuffList;

        if (targetList != null && targetList.Count > 0)
        {
            int last = targetList.Count - 1;
            GameObject item = targetList[last];
            targetList.RemoveAt(last);
            
            UpdateAllLayouts(); // 하나 뺐으니 즉시 정렬
            return item;
        }
        return null;
    }

    // 매개변수에 duration을 추가하여 속도 제어 가능하게 수정
    IEnumerator SmoothMove(GameObject obj, Vector3 localPos, float duration = 0.1f)
    {
        float elapsed = 0f;
        Vector3 startPos = obj.transform.localPosition;
        Quaternion startRot = obj.transform.localRotation;

        while (elapsed < duration)
        {
            if (obj == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            obj.transform.localPosition = Vector3.Lerp(startPos, localPos, t);
            obj.transform.localRotation = Quaternion.Lerp(startRot, Quaternion.identity, t);
            yield return null;
        }
        if (obj != null) obj.transform.localPosition = localPos;
    }

    // public void AddAndSortResource(GameObject newObj)
    // {
    //     if (collectedObjects.Count >= maxCapacity) return;

    //     // 1. 물리 비활성화
    //     if (newObj.TryGetComponent<Collider>(out Collider col)) col.enabled = false;
    //     newObj.transform.SetParent(stackPoint);

    //     // 2. 리스트에 추가
    //     collectedObjects.Add(newObj);

    //     // 3. 우선순위에 따라 리스트 정렬
    //     collectedObjects.Sort((a, b) => GetPriority(a.tag).CompareTo(GetPriority(b.tag)));

    //     // 4. 정렬된 순서대로 위치 재배치
    //     ReorderStack();
    // }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("OutputArea") && collectionCoroutine != null)
        {
            StopCoroutine(collectionCoroutine);
            collectionCoroutine = null;
        }
    }

    // private IEnumerator CollectHandcuffsRoutine(ResourceConverter converter)
    // {
    //     while (true)
    //     {
    //         // Debug.Log("수집 시도 중...");

    //         // 1. 플레이어 소지량이 꽉 찼는지 확인
    //         if (collectedObjects.Count < maxCapacity)
    //         {
    //             // 2. 변환기에서 수갑을 하나 요청
    //             GameObject handcuff = converter.TakeHandcuff();

    //             if (handcuff != null)
    //             {
    //                 // 3. 내 뒤에 쌓기
    //                 AddSpecificResource(handcuff);
                    
    //                 // 4. 수집 간격 (0.1~0.2초 정도가 적당합니다)
    //                 yield return new WaitForSeconds(0.15f);
    //             }
    //             else
    //             {
    //                 // 변환기에 수갑이 없으면 잠시 대기
    //                 yield return new WaitForSeconds(0.2f);
    //             }
    //         }
    //         else
    //         {
    //             // 꽉 찼으면 MAX 표시 (기존 함수 활용)
    //             ShowMaxText();
    //             yield return new WaitForSeconds(1.0f);
    //         }
    //     }
    // }

    // --- 아이템 추출 (데스크, 변환기 등에서 사용) ---
    public GameObject PopSpecificItem(string tag)
    {
        List<GameObject> targetList = (tag == "Coal") ? coalList : (tag == "Money") ? moneyList : (tag == "Handcuff") ? handcuffList : null;

        if (targetList != null && targetList.Count > 0)
        {
            int lastIndex = targetList.Count - 1;
            GameObject item = targetList[lastIndex];
            targetList.RemoveAt(lastIndex);
            UpdateAllLayouts();
            return item;
        }
        return null;
    }

    // 아이템별 우선순위 (낮을수록 플레이어와 가까움)
    private int GetPriority(string tag)
    {
        switch (tag)
        {
            case "Coal": return 1;
            case "Money": return 2;
            case "Handcuff": return 3;
            default: return 4;
        }
    }


}