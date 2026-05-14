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
        Destroy(gameObject, 5f);
    }
}