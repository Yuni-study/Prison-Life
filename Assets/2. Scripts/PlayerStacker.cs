using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerStacker : MonoBehaviour
{
    [Header("Anchors")]
    public Transform stackPoint;        // 모든 앵커들의 부모 
    public Transform coalPoint;
    public Transform moneyPoint;
    public Transform handcuffPoint;

    [Header("Settings")]
    public float verticalOffset = 0.25f; // 위로 쌓이는 간격
    public float horizontalOffset = 0.6f; // 구역 간의 최소 간격
    public int maxCapacity = 20;        // 최대 소지량

    // 아이템 종류별 리스트
    private List<GameObject> coalList = new List<GameObject>();
    private List<GameObject> moneyList = new List<GameObject>();
    private List<GameObject> handcuffList = new List<GameObject>();

    // 전후 순서 관리 리스트 
    List<Transform> pointOrderList = new List<Transform>(); 

    [Header("UI")]
    public GameObject maxTextUI;        // MAX 텍스트 오브젝트

    private Coroutine collectionCoroutine;

    private int TotalCount => coalList.Count + moneyList.Count + handcuffList.Count;

    // 현재 소지하고 있는 모든 아이템(석탄, 돈, 수갑)의 총합을 반환
    public int GetItemCount()
    {
        return coalList.Count + moneyList.Count + handcuffList.Count;
    }

    [Header("Economy")]
    public int currentMoney = 0;

    public bool IsFull() => TotalCount >= maxCapacity;

    private int _currentMoney;
    public int CurrentMoney
    {
        get => _currentMoney;
        set {
            _currentMoney = value;
            UIManager.Instance.UpdateMoneyUI(_currentMoney); // 값이 변할 때마다 UI 갱신
        }
    }

    private void Awake()
    {
        pointOrderList.Add(coalPoint);
        pointOrderList.Add(moneyPoint);
        pointOrderList.Add(handcuffPoint);
    }

    // new
    private void _UpdatePointOrderList(Transform activePoint)
    {
        // activePoint가 이미 맨 앞에 있다면 -> 순서 변경 불필요
        if(pointOrderList.IndexOf(activePoint) == 0) return; 

        // 기존 앵커를 pointOrderList에서 제거한 후 맨 앞으로 삽입 (순서 변경)
        pointOrderList.Remove(activePoint);
        pointOrderList.Insert(0, activePoint); 

        // 변경된 순서에 맞춰 재배치 
        _UpdatePointOrderList();
    }

    private void _UpdatePointOrderList()
    {
        for(int i = 0; i < pointOrderList.Count; i++)
        {
            Transform point = pointOrderList[i];

            /*
            인덱스 0 : z = 0 * -0.6 = 0
            인덱스 1 : z = 1 * -0.6 = -0.6
            인덱스 2 : z = 2 * -0.6 = -1.2
            */
            float zPos = i * -horizontalOffset;
            Vector3 localPos = new Vector3(0, 0, zPos);

            StartCoroutine(SmoothMove(point.gameObject, localPos, 0.2f));
        }
    }

    IEnumerator SmoothMove(GameObject obj, Vector3 localPos, float duration)
    {
        if (obj == null) yield break;

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

    // 또는 특정 종류만 세고 싶을 때를 대비해 아래처럼 만들 수도 있습니다 (선택사항)
    public int GetSpecificItemCount(string tag)
    {
        if (tag == "Coal") return coalList.Count;
        if (tag == "Money") return moneyList.Count;
        if (tag == "Handcuff") return handcuffList.Count;
        return 0;
    }

    public void AddItemToList(GameObject obj, string tag)
    {
        if (obj == null) return;

        // 1. 소지량 체크
        if (IsFull())
        {
            ShowMaxText();
            // 씬에 이미 배치된 오브젝트가 아니라 새로 생성된 프리팹인 경우만 파괴
            if (obj.scene.name == null) Destroy(obj); 
            return;
        }

        // 2. 물리 충돌 비활성화 (중복 획득 방지)
        if (obj.TryGetComponent<Collider>(out Collider col)) col.enabled = false;

        Transform activePoint = null;

        // 3. 태그별 리스트 추가 및 부모 설정
        switch (tag)
        {
            case "Coal": 
                coalList.Add(obj); 
                obj.transform.SetParent(coalPoint);
                activePoint = coalPoint;
                break;
            case "Money": 
                moneyList.Add(obj); 
                obj.transform.SetParent(moneyPoint);
                activePoint = moneyPoint;
                break;
            case "Handcuff": 
                handcuffList.Add(obj); 
                obj.transform.SetParent(handcuffPoint);
                activePoint = handcuffPoint;
                break;
        }

        // 앵커 순서 업데이트 (가장 최근에 추가된 아이템이 있는 구역이 앞으로 오도록)
        if(activePoint != null)
        {
            _UpdatePointOrderList(activePoint);
        }

        // 4. 즉시 레이아웃 업데이트
        UpdateAllLayouts();
    }

    public void ShowMaxText() 
    { 
        if(maxTextUI != null && !maxTextUI.activeSelf) 
        { 
            maxTextUI.SetActive(true);
            CancelInvoke("HideMaxText"); // 이전 예약 취소
            Invoke("HideMaxText", 1f);
        } 
    }

    private void HideMaxText()
    {
        maxTextUI.SetActive(false);
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Money"))
        {
            // 데스크에서 돈을 가져오는 경우
            other.GetComponentInParent<DeskManager>()?.moneyOnDesk.Remove(other.gameObject);
            AddItemToList(other.gameObject, "Money");

            AddMoney(100); // 돈 하나당 100원으로 가정
        }
        else if (other.CompareTag("OutputArea"))
        {
            // 수갑 배출구에 들어갔을 때
            ResourceConverter converter = other.GetComponentInParent<ResourceConverter>();
            if (converter != null)
            {
                if (collectionCoroutine != null) StopCoroutine(collectionCoroutine);
                collectionCoroutine = StartCoroutine(CollectRoutine(converter));
            }
        }
    }

    IEnumerator CollectRoutine(ResourceConverter converter)
    {
        while (true)
        {
            if (IsFull())
            {
                ShowMaxText();
                yield return new WaitForSeconds(0.5f); // 꽉 찼을 땐 체크 주기 늦춤
                continue;
            }

            GameObject item = converter.TakeHandcuff();
            if (item != null)
            {
                AddItemToList(item, "Handcuff");
                SoundManager.Instance.PlaySFX(SoundManager.Instance.collectClip);
                yield return new WaitForSeconds(0.05f); // 수집 속도
            }
            else
            {
                // 변환기에 수갑이 없으면 잠시 대기
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    // 핵심: 모든 구역의 위치와 아이템들의 위치를 재정렬
    public void UpdateAllLayouts()
    {
        // 1. 석탄 (중앙)
        SortZone(coalList);

        // 2. 돈 (왼쪽)
        SortZone(moneyList);

        // 3. 수갑 (오른쪽)
        SortZone(handcuffList);
    }

    private void SortZone(List<GameObject> list)
    {
        for(int i = 0; i < list.Count; i++)
        {
            if(list[i] == null) continue; 

            Vector3 targetPos = new Vector3(0, i * verticalOffset, 0);

            // y축으로 쌓기. 
            StartCoroutine(SmoothMove(list[i], targetPos, 0.1f));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("OutputArea") && collectionCoroutine != null)
        {
            StopCoroutine(collectionCoroutine);
            collectionCoroutine = null;
        }
    }

    public GameObject PopSpecificItem(string tag)
    {
        List<GameObject> targetList = (tag == "Coal") ? coalList : (tag == "Money") ? moneyList : (tag == "Handcuff") ? handcuffList : null;

        if (targetList != null && targetList.Count > 0)
        {
            int lastIndex = targetList.Count - 1;
            GameObject item = targetList[lastIndex];

            targetList.RemoveAt(lastIndex);

            // 앵커 정렬 
            if(targetList.Count == 0)
            {
                Transform emptyPoint = (tag == "Coal") ? coalPoint : (tag == "Money") ? moneyPoint : (tag == "Handcuff") ? handcuffPoint : null;

                if(emptyPoint != null)
                {
                    pointOrderList.Remove(emptyPoint);
                    pointOrderList.Add(emptyPoint);

                    _UpdatePointOrderList();
                }
            }
            
            // 아이템이 빠졌으므로 즉시 나머지 정렬
            UpdateAllLayouts();

            return item;
        }
        return null;
    }

    // 돈을 획득하는 시점 (예: OnTriggerEnter에서 Money 태그 오브젝트 닿았을 때)
    public void AddMoney(int amount)
    {
        CurrentMoney += amount;
    }
}