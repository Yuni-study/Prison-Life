using UnityEngine;
using System.Collections.Generic;

public class PrisonerManager : MonoBehaviour
{
    public GameObject prisonerPrefab;
    public DeskManager deskManager; 
    
    [Header("Points")]
    public Transform gatePoint;    // 소환 위치
    public Transform waitPoint;    // 2등 대기석
    public Transform servicePoint; // 1등 (데스크 앞)
    public Transform prisonPoint;  // 일 다 보고 가는 곳

    private List<Prisoner> queue = new List<Prisoner>();

    void Start()
    {
        // 시작하자마자 2명을 소환합니다.
        for(int i = 0; i < 2; i++)
        {
            SpawnPrisoner();
        }
    }

    void Update()
    {
        // 누군가 나가서 2명보다 적어지면 새로 소환
        if (queue.Count < 2)
        {
            SpawnPrisoner();
        }
    }

    void SpawnPrisoner()
    {
        if (prisonerPrefab == null) return;

        GameObject obj = Instantiate(prisonerPrefab, gatePoint.position, Quaternion.identity);
        Prisoner p = obj.GetComponent<Prisoner>();
        
        if (p == null)
        {
            Debug.LogError("소환된 프리팹에 Prisoner 스크립트가 없습니다!");
            return;
        }

        queue.Add(p);
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

        for (int i = 0; i < queue.Count; i++)
        {
            // 리스트 안의 죄수 자체가 혹시 null인지 체크
            if (queue[i] == null) continue;

            if (i == 0) {
                if (servicePoint != null)
                {
                    queue[i].SetDestination(servicePoint.position);
                    deskManager.SetTargetPrisoner(queue[i]); 
                }
            }
            else if (i == 1) {
                if (waitPoint != null)
                {
                    queue[i].SetDestination(waitPoint.position);
                }
            }
        }
    }

    public void DismissPrisoner()
    {
        if (queue.Count > 0)
        {
            Prisoner p = queue[0];
            queue.RemoveAt(0);
            
            // 죄수에게 감옥 위치를 주며 떠나라고 명령
            p.GetServed(prisonPoint.position);
            
            // 뒷사람 전진!
            ArrangeQueue(); 
        }
    }
}