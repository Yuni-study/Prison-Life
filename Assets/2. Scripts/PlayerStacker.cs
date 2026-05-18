using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class PlayerStacker : MonoBehaviour
{
    [Header("Anchors")]
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
    private List<GameObject> _coalList = new List<GameObject>();
    private List<GameObject> _moneyList = new List<GameObject>();
    private List<GameObject> _handcuffList = new List<GameObject>();

    // 앵커 순서 관리 리스트 
    private List<Transform> _pointOrderList = new List<Transform>(); 

    private Dictionary<Transform, Coroutine> _anchorCoroutineDict = new Dictionary<Transform, Coroutine>();

    [Header("Smooth Move Settings")]
    private float _smoothMoveDuration = 0.2f;

    private TextMeshPro _maxText;
    private Coroutine _collectionCoroutine;

    [Header("Sway Settings")]
    private float _swayIntensity = 0.05f; // 휘어지는 강도 
    private float _swayMaxAngle = 15.0f; // 최대 휘어짐 제한 
    private float _returnSpeed = 5.0f; // 원래대로 돌아오는 복원 속도 

    private Vector3 _previousPosition; // 이전 프레임 위치 
    private Vector3 _moveVelocity; // 플레이어 실제 이동 속도 벡터 

    public int TotalCount => _coalList.Count + _moneyList.Count + _handcuffList.Count; 

    private UIManager _uiManager;

    // 수갑이 꽉 찼을 때 MAX텍스트 보여주는 시간, 수갑 수집 시간, 수갑이 없을 때 대기 시간 0.5,0.05,0.2
    private WaitForSeconds _maxTextShowDuration;
    private WaitForSeconds _handCuffCollectDuration;
    private WaitForSeconds _waitDuration;

    private void Awake()
    {
        _pointOrderList.Add(coalPoint);
        _pointOrderList.Add(moneyPoint);
        _pointOrderList.Add(handcuffPoint);

        _previousPosition = transform.position;

        _maxText = GetComponentInChildren<TextMeshPro>(true);

        _maxTextShowDuration = new WaitForSeconds(Constants.POINTFIVE);
        _handCuffCollectDuration = new WaitForSeconds(Constants.POINTZEROFIVE);
        _waitDuration = new WaitForSeconds(Constants.POINTTWO);
    }

    private void Start()
    {
        _uiManager = UIManager.Instance;
    }

    public void SetInertiaSettings(float swayIntensity, float swayMaxAngle, float returnSpeed)
    {
        _swayIntensity = swayIntensity;
        _swayMaxAngle = swayMaxAngle;
        _returnSpeed = returnSpeed;
    }

    // 현재 소지하고 있는 모든 아이템(석탄, 돈, 수갑)의 총합을 반환
    public int GetItemCount() => TotalCount;

    public bool IsFull() => TotalCount >= maxCapacity;
    public bool IsFull(string tag)
    {
        switch (tag)
        {
            case "Coal" : 
                return _coalList.Count >= maxCoalCapacity;
            case "Money" : 
                return _moneyList.Count >= maxMoneyCapacity;
            case "Handcuff" : 
                return _handcuffList.Count >= maxHandcuffCapacity;
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
            _uiManager.OnMoneyChanged?.Invoke(_currentMoney);
        }
    }

    public bool IsSortingAnchors
    {
        get
        {
            foreach(var anchorCoroutine in _anchorCoroutineDict)
            {
                if(anchorCoroutine.Value != null) return true;
            }
            return false;
        }
    }

    private void _UpdatePointOrderList(Transform activePoint)
    {
        // activePoint가 이미 맨 앞에 있다면 -> 순서 변경 X
        if(_pointOrderList.IndexOf(activePoint) == Constants.ZERO_INTEGER) return; 

        // activePoint를 pointOrderList에서 제거한 후 맨 앞으로 삽입 (순서 변경)
        _pointOrderList.Remove(activePoint);
        _pointOrderList.Insert(0, activePoint); 

        // 변경된 순서에 맞춰 재배치 
        _UpdatePointOrderList();
    }

    private void _UpdatePointOrderList()
    {
        for(int i = 0; i < _pointOrderList.Count; i++)
        {
            Transform point = _pointOrderList[i];

            /*
            인덱스 0 : z = 0 * -0.6 = 0
            인덱스 1 : z = 1 * -0.6 = -0.6
            인덱스 2 : z = 2 * -0.6 = -1.2
            */
            float zPos = i * -horizontalOffset;
            Vector3 endLocalPos = new Vector3(Constants.ZERO_FLOAT, Constants.ZERO_FLOAT, zPos);

            // point가 이미 SmoothMove 중이라면(이전 SmoothMove가 진행중이라면) 멈추기. (떨림현상 방지)
            if(_anchorCoroutineDict.TryGetValue(point, out Coroutine activeCoroutine))
            {
                if(activeCoroutine != null)
                {
                    StopCoroutine(activeCoroutine);
                }
            }

            _anchorCoroutineDict[point] = StartCoroutine(_SmoothMove(point, endLocalPos, _smoothMoveDuration));
            // StartCoroutine(SmoothMove(point.gameObject, localPos, 0.2f));
        }
    }

    private IEnumerator _SmoothMove(Transform startTransform, Vector3 endLocalPos, float duration)
    {
        if(startTransform == null) yield break;

        float elapsed = Constants.ZERO_FLOAT;
        Vector3 startPos = startTransform.localPosition;
        Quaternion startRot = startTransform.localRotation;

        while(elapsed < duration)
        {
            if(startTransform == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            startTransform.localPosition = Vector3.Lerp(startPos, endLocalPos, t);
            startTransform.localRotation = Quaternion.Lerp(startRot, Quaternion.identity, t);
            yield return null;
        }

        if(startTransform != null)
        {
            startTransform.localPosition = endLocalPos;
            startTransform.localRotation = Quaternion.identity;

            _anchorCoroutineDict[startTransform] = null;
        }
    }

    // 또는 특정 종류만 세고 싶을 때를 대비해 아래처럼 만들 수도 있습니다 (선택사항)
    public int GetSpecificItemCount(string tag)
    {
        if (tag == "Coal") return _coalList.Count;
        if (tag == "Money") return _moneyList.Count;
        if (tag == "Handcuff") return _handcuffList.Count;
        return Constants.ONE_INTEGER;
    }

    public void AddItemToList(GameObject obj, string tag)
    {
        if (obj == null) return;

        if(IsFull(tag))
        {
            ShowMaxText();
            return;
        }

        // 2. 물리 충돌 비활성화 (중복 획득 방지)
        if (obj.TryGetComponent<Collider>(out Collider col)) col.enabled = false;

        Transform activePoint = null;

        // 3. 태그별 리스트 추가 및 부모 설정
        switch (tag)
        {
            case "Coal": 
                _coalList.Add(obj); 
                obj.transform.SetParent(coalPoint);
                activePoint = coalPoint;
                break;
            case "Money": 
                _moneyList.Add(obj); 
                obj.transform.SetParent(moneyPoint);
                activePoint = moneyPoint;
                break;
            case "Handcuff": 
                _handcuffList.Add(obj); 
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
        if(_maxText != null && !_maxText.gameObject.activeSelf) 
        { 
            _maxText.gameObject.SetActive(true);
            CancelInvoke(Constants.HIDEMAXTEXT); 
            Invoke(Constants.HIDEMAXTEXT, Constants.ONE_FLOAT);
        } 
    }

    private void HideMaxText()
    {
        if(_maxText != null)
        {
            _maxText.gameObject.SetActive(false);
        }
    }

    // 석탄만 가지고 있는지 확인하는 함수 (변환기용)
    public bool HasCoal() => _coalList.Count > Constants.ZERO_INTEGER;

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
            if (IsFull("Money"))
            {
                ShowMaxText();
                return;
            }

            // 데스크에서 돈을 가져오는 경우
            other.GetComponentInParent<Desk>()?.moneyOnDesk.Remove(other.gameObject);
            AddItemToList(other.gameObject, "Money");

            AddMoney(Constants.ONEHUNDRED_INTEGER); // 돈 하나당 100원으로 가정
        }
        else if (other.CompareTag("OutputArea"))
        {
            // 수갑 배출구에 들어갔을 때
            ResourceConverter converter = other.GetComponentInParent<ResourceConverter>();
            if (converter != null)
            {
                if (_collectionCoroutine != null) StopCoroutine(_collectionCoroutine);
                _collectionCoroutine = StartCoroutine(_CollectRoutine(converter));
            }
        }
    }

    private IEnumerator _CollectRoutine(ResourceConverter converter)
    {
        while (true)
        {
            if (IsFull("Handcuff"))
            {
                ShowMaxText();
                yield return _maxTextShowDuration; // 수갑 구역이 꽉 찼을 땐 체크 주기 늦춤   
                continue;
            }

            GameObject item = converter.TakeHandcuff();
            if (item != null)
            {
                AddItemToList(item, "Handcuff");
                SoundManager.Instance.PlaySFX(SoundManager.Instance.collectClip);
                yield return _handCuffCollectDuration; // 수집 속도
            }
            else
            {
                // 변환기에 수갑이 없으면 잠시 대기
                yield return _waitDuration;
            }
        }
    }

    // 핵심: 모든 구역의 위치와 아이템들의 위치를 재정렬
    public void UpdateAllLayouts(Vector3 localVelocity)
    {
        // 1. 석탄 (중앙)
        _SortZone(_coalList, localVelocity);

        // 2. 돈 (왼쪽)
        _SortZone(_moneyList, localVelocity);

        // 3. 수갑 (오른쪽)
        _SortZone(_handcuffList, localVelocity);
    }

    private void _SortZone(List<GameObject> list, Vector3 localVelocity)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null) continue;

            float targetY = i * verticalOffset;

            // ★ 핵심 수정: '로컬 속도'의 정반대 방향으로 관성 힘(sway)을 계산합니다.
            // 캐릭터가 로컬 앞(Z+)으로 가면 오프셋은 뒤(Z-)로, 로컬 오른쪽(X+)으로 가면 오프셋은 왼쪽(X-)이 됩니다.
            Vector3 swayDirection = -localVelocity * _swayIntensity;

            // 위로 갈수록 더 많이 휘어지는 포물선 공식 (기존과 동일)
            Vector3 swayOffset = swayDirection * (i * Constants.POINTFIVE); 

            // 최종 로컬 목표 위치
            Vector3 targetPos = new Vector3(swayOffset.x, targetY, swayOffset.z);

            // 프레임 떨림 방지를 위한 부드러운 이동
            list[i].transform.localPosition = Vector3.Lerp(
                list[i].transform.localPosition, 
                targetPos, 
                Time.deltaTime * Constants.TEN_FLOAT
            );

            // [회전 연출 수정] 회전 역시 로컬 swayDirection을 기반으로 처리하여 방향 통일
            if (localVelocity.magnitude > Constants.POINTONE)
            {
                // 로컬 축 기준으로 비스듬히 눕는 회전값 계산
                Quaternion targetRot = Quaternion.Euler(swayDirection.z * _swayMaxAngle, Constants.ZERO_INTEGER, -swayDirection.x * _swayMaxAngle);
                list[i].transform.localRotation = Quaternion.Lerp(list[i].transform.localRotation, targetRot, Time.deltaTime * Constants.TEN_FLOAT);
            }
            else
            {
                list[i].transform.localRotation = Quaternion.Lerp(list[i].transform.localRotation, Quaternion.identity, Time.deltaTime * _returnSpeed);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("OutputArea") && _collectionCoroutine != null)
        {
            StopCoroutine(_collectionCoroutine);
            _collectionCoroutine = null;
        }
    }

    public GameObject PopSpecificItem(string tag)
    {
        List<GameObject> targetList = (tag == "Coal") ? _coalList : (tag == "Money") ? _moneyList : (tag == "Handcuff") ? _handcuffList : null;

        if (targetList != null && targetList.Count > Constants.ZERO_INTEGER)
        {
            int lastIndex = targetList.Count - Constants.ONE_INTEGER;
            GameObject item = targetList[lastIndex];

            targetList.RemoveAt(lastIndex);

            // 앵커 정렬 
            if(targetList.Count == Constants.ZERO_INTEGER)
            {
                Transform emptyPoint = (tag == "Coal") ? coalPoint : (tag == "Money") ? moneyPoint : (tag == "Handcuff") ? handcuffPoint : null;

                if(emptyPoint != null)
                {
                    _pointOrderList.Remove(emptyPoint);
                    _pointOrderList.Add(emptyPoint);

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