using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Prisoner : MonoBehaviour
{
    public int needHandcuffs; 
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // 1~3개 사이의 랜덤한 요구량 설정
        needHandcuffs = Random.Range(1, 4);
        Debug.Log($"요구 수갑 = {needHandcuffs}");
    }

    public void SetDestination(Vector3 targetPos)
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(targetPos);
    }

    // DeskManager에서 호출할 메서드
    public void GetServed(Vector3 escapePos)
    {
        // 목적지를 감옥/탈출구로 설정
        SetDestination(escapePos);

        // 5초 뒤에 오브젝트 삭제 (이동할 시간 확보)
        // Destroy(gameObject, 5f);

        // 감옥 도착 후 처리 (옵션)
        StartCoroutine(ArrivedAtCell());
    }

    IEnumerator ArrivedAtCell()
    {
        // 목적지 근처에 도착할 때까지 대기
        while (agent.pathPending || agent.remainingDistance > 0.1f)
        {
            yield return null;
        }
        // 도착하면 AI를 끄고 정지 상태 애니메이션으로 변경
        agent.isStopped = true;
        agent.enabled = false;
        // transform.rotation = prisonStartPoint.rotation; // 방향 정렬 필요 시
    }

    public void MoveToPrison(Vector3 targetPos, bool isFinalUpgradeDone)
    {
        agent.SetDestination(targetPos);
        
        // 업그레이드 전이라면 일정 시간 후 사라지게 함
        if (!isFinalUpgradeDone)
        {
            StartCoroutine(DisappearAfterArrival());
        }
    }

    IEnumerator DisappearAfterArrival()
    {
        // 목적지에 거의 도착할 때까지 대기
        while (agent.remainingDistance > 0.2f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(2f); // 감옥 안에서 2초 정도 대기 후
        // Destroy(gameObject); // 죄수 오브젝트 삭제
    }
}