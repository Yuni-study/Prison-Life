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
    public int maxCoalCapacity = 20;
    public int maxMoneyCapacity = 20;
    public int maxHandcuffCapacity = 20;

    // 아이템 종류별 리스트
    private List<GameObject> coalList = new List<GameObject>();
    private List<GameObject> moneyList = new List<GameObject>();
    private List<GameObject> handcuffList = new List<GameObject>();

    // 전후 순서 관리 리스트 
    List<Transform> pointOrderList = new List<Transform>(); 

    [Header("UI")]
    public GameObject maxTextUI;        // MAX 텍스트 오브젝트

    private Coroutine collectionCoroutine;

    [Header("Sway Settings")]
    public float swayIntensity = 0.05f; // 휘어지는 강도 
    public float swayMaxAngle = 15.0f; // 최대 휘어짐 제한 
    public float returnSpeed = 5.0f; // 원래대로 돌아오는 복원 속도 

    private Vector3 previousPosition; // 이전 프레임 위치 
    private Vector3 moveVelocity; // 플레이어 실제 이동 속도 벡터 

    private int TotalCount => coalList.Count + moneyList.Count + handcuffList.Count;

    // 현재 소지하고 있는 모든 아이템(석탄, 돈, 수갑)의 총합을 반환
    public int GetItemCount()
    {
        return coalList.Count + moneyList.Count + handcuffList.Count;
    }

    public bool IsFull() => TotalCount >= maxCapacity;
    public bool IsFull(string tag)
    {
        switch (tag)
        {
            case "Coal" : 
                return coalList.Count >= maxCoalCapacity;
            case "Money" : 
                return moneyList.Count >= maxMoneyCapacity;
            case "Handcuff" : 
                return handcuffList.Count >= maxHandcuffCapacity;
            default : 
                return false;
        }
    }

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

        previousPosition = transform.position;
    }

    private void Update()
    {
        // 1. 프레임간 위치 변화를 바탕으로 이동 속도 벡터 계산 (X, Z축만 사용)
        Vector3 currentPos = transform.position;
        Vector3 rawVelocity = (currentPos - previousPosition) / Time.deltaTime;
        
        // Y축 높이 변화는 무시하고 수평 이동만 계산
        rawVelocity.y = 0; 

        // 속도 변화를 부드럽게 보간 (급격하게 튀는 현상 방지)
        moveVelocity = Vector3.Lerp(moveVelocity, rawVelocity, Time.deltaTime * returnSpeed);
        previousPosition = currentPos;

        Vector3 localVelocity = transform.InverseTransformDirection(moveVelocity);

        // 2. 이동 중일 때 실시간으로 아이템들의 포물선 위치 업데이트
        if (TotalCount > 0)
        {
            UpdateAllLayouts(localVelocity);
        }
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
        // if (IsFull())
        // {
        //     ShowMaxText();
        //     // 씬에 이미 배치된 오브젝트가 아니라 새로 생성된 프리팹인 경우만 파괴
        //     if (obj.scene.name == null) Destroy(obj); 
        //     return;
        // }
        if(IsFull(tag))
        {
            ShowMaxText();
            if(obj.scene.name == null) Destroy(obj);
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
        UpdateAllLayouts(Vector3.zero);
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
            // if (IsFull())
            // {
            //     ShowMaxText();
            //     yield return new WaitForSeconds(0.5f); // 꽉 찼을 땐 체크 주기 늦춤
            //     continue;
            // }
            if (IsFull("Handcuff"))
            {
                ShowMaxText();
                yield return new WaitForSeconds(0.5f); // 수갑 구역이 꽉 찼을 땐 체크 주기 늦춤
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
    public void UpdateAllLayouts(Vector3 localVelocity)
    {
        // 1. 석탄 (중앙)
        SortZone(coalList, localVelocity);

        // 2. 돈 (왼쪽)
        SortZone(moneyList, localVelocity);

        // 3. 수갑 (오른쪽)
        SortZone(handcuffList, localVelocity);
    }

    private void SortZone(List<GameObject> list, Vector3 localVelocity)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null) continue;

            float targetY = i * verticalOffset;

            // ★ 핵심 수정: '로컬 속도'의 정반대 방향으로 관성 힘(sway)을 계산합니다.
            // 캐릭터가 로컬 앞(Z+)으로 가면 오프셋은 뒤(Z-)로, 로컬 오른쪽(X+)으로 가면 오프셋은 왼쪽(X-)이 됩니다.
            Vector3 swayDirection = -localVelocity * swayIntensity;

            // 위로 갈수록 더 많이 휘어지는 포물선 공식 (기존과 동일)
            Vector3 swayOffset = swayDirection * (i * 0.5f); 

            // 최종 로컬 목표 위치
            Vector3 targetPos = new Vector3(swayOffset.x, targetY, swayOffset.z);

            // 프레임 떨림 방지를 위한 부드러운 이동
            list[i].transform.localPosition = Vector3.Lerp(
                list[i].transform.localPosition, 
                targetPos, 
                Time.deltaTime * 10f
            );

            // [회전 연출 수정] 회전 역시 로컬 swayDirection을 기반으로 처리하여 방향 통일
            if (localVelocity.magnitude > 0.1f)
            {
                // 로컬 축 기준으로 비스듬히 눕는 회전값 계산
                Quaternion targetRot = Quaternion.Euler(swayDirection.z * swayMaxAngle, 0, -swayDirection.x * swayMaxAngle);
                list[i].transform.localRotation = Quaternion.Lerp(list[i].transform.localRotation, targetRot, Time.deltaTime * 10f);
            }
            else
            {
                list[i].transform.localRotation = Quaternion.Lerp(list[i].transform.localRotation, Quaternion.identity, Time.deltaTime * returnSpeed);
            }
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
            UpdateAllLayouts(Vector3.zero);

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