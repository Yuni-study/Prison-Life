using UnityEngine;
using System.Collections.Generic;

public class PrisonerManager : Singleton_Mono<PrisonerManager>
{
    public GameObject prisonerPrefab;
    public Desk deskManager; 
    
    [Header("Points")]
    public Transform gatePoint;    // 소환 위치
    public Transform waitPoint;    // 2등 대기석
    public Transform servicePoint; // 1등 (데스크 앞)
    public Transform prisonPoint;  // 일 다 보고 가는 곳
    public Transform prisonStartPoint; // [추가] 감옥 내부 정렬 시작점

    private List<Prisoner> _queue = new List<Prisoner>();
    private List<GameObject> _inPrisonList = new List<GameObject>(); // [추가] 감옥 안 죄수들

    [Header("Prison Settings")]
    public int maxPrisonersInPrison = 20; // 최대 수용량
    public Vector2 spacing = new Vector2(1.5f, 1.5f); // 죄수 간 간격 (가로, 세로)
    public int rowCount = 5; // 한 줄에 5명씩

    [Header("State")]
    public bool isFinalUpgradeComplete = false;

    [Header("Prison Objects")]
    public GameObject basicJailObject; // 처음엔 꺼져 있다가 20명 되면 켜질 기본 감옥

    private void Start()
    {
        // 시작하자마자 2명을 소환
        for(int i = 0; i < 2; i++)
        {
            _SpawnPrisoner();
        }
    }

    private void Update()
    {
        // 누군가 나가서 2명보다 적어지면 새로 소환
        if (_queue.Count < 2)
        {
            _SpawnPrisoner();
        }
    }

    // 감옥이 꽉 찼는지 확인
    public bool IsPrisonFull()
    {
        // 최종 업그레이드가 끝났을 때만 'Full' 상태를 체크함
        if (isFinalUpgradeComplete)
            return _inPrisonList.Count >= maxPrisonersInPrison;
        
        // 업그레이드 전에는 죄수가 계속 사라지므로 항상 수용 가능함
        return false;
    }
    public int GetCurrentPrisonerCount() => _inPrisonList.Count;

    private void _SpawnPrisoner()
    {
        if (prisonerPrefab == null) return;

        GameObject obj = Instantiate(prisonerPrefab, gatePoint.position, Quaternion.identity);
        Prisoner p = obj.GetComponent<Prisoner>();
        
        if (p == null)
        {
            Debug.LogError("소환된 프리팹에 Prisoner 스크립트가 없습니다!");
            return;
        }

        _queue.Add(p);
        ArrangeQueue();
    }

    public void ArrangeQueue()
    {
        // deskManager가 할당되지 않았으면 실행하지 않음
        if (deskManager == null)
        {
            Debug.LogError("PrisonerManager: DeskManager가 인스펙터에서 할당되지 않았습니다!");
            return;
        }

        for (int i = 0; i < _queue.Count; i++)
        {
            // 리스트 안의 죄수 자체가 혹시 null인지 체크
            if (_queue[i] == null) continue;

            if (i == 0) {
                if (servicePoint != null)
                {
                    _queue[i].SetDestination(servicePoint.position);
                    deskManager.SetTargetPrisoner(_queue[i]);
                }
            }
            else if (i == 1) {
                if (waitPoint != null)
                {
                    _queue[i].SetDestination(waitPoint.position);
                }
            }
        }
    }

    public void DismissPrisoner()
    {
        if (_queue.Count > 0)
        {
            Prisoner p = _queue[0];
            _queue.RemoveAt(0);
            
            // // 죄수에게 감옥 위치를 주며 떠나라고 명령

            // 파괴하지 않고 감옥 내부 정렬 위치로 이동
            _MoveToPrisonCell(p);
            
            // 뒷사람 전진
            ArrangeQueue(); 
        }
    }

    private void _MoveToPrisonCell(Prisoner p)
    {
        // 현재 감옥에 있는 인원수가 설정된 최대치(20명)보다 적은지 확인
        if (_inPrisonList.Count < maxPrisonersInPrison)
        {
            // 1. 감옥에 자리가 있는 경우: 리스트에 추가하고 정해진 자리에 배치 (영구 보존)
            _inPrisonList.Add(p.gameObject);

            if (_inPrisonList.Count == maxPrisonersInPrison)
            {
                if(basicJailObject != null) basicJailObject.SetActive(true);
                Debug.Log("죄수 20명 수감 완료! 기본 감옥이 나타났습니다.");
            }
            
            int index = _inPrisonList.Count - 1;
            int row = index / rowCount;
            int col = index % rowCount;

            Vector3 targetPos = prisonStartPoint.position + new Vector3(col * spacing.x, 0, -row * spacing.y);
            
            // Prisoner 스크립트의 GetServed 호출 (이동 명령)
            p.GetServed(targetPos); 
            
            Debug.Log($"죄수 수감 완료: {_inPrisonList.Count}/{maxPrisonersInPrison}");
        }
        else
        {
            // 2. 감옥이 이미 20명으로 꽉 찬 경우: 감옥 안쪽으로 보내고 5초 뒤 파괴
            p.GetServed(prisonPoint.position);
            
            // 안쪽으로 걸어 들어가는 모습을 보여준 뒤 삭제
            Destroy(p.gameObject, 5f); 
            
            Debug.Log("감옥 만원: 죄수가 수갑만 받고 사라집니다.");
        }
    }
}