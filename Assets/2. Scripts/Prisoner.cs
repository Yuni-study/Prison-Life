using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Prisoner : MonoBehaviour
{
    public int needHandcuffs; 
    private NavMeshAgent _agent;

    private WaitForSeconds _waitDuration;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        // 1~3개 사이의 랜덤한 요구량 설정
        needHandcuffs = Random.Range(1, 4);
        Debug.Log($"요구 수갑 = {needHandcuffs}");

        _waitDuration = new WaitForSeconds(Constants.TWO_FLOAT);
    }

    public void SetDestination(Vector3 targetPos)
    {
        if (_agent == null) _agent = GetComponent<NavMeshAgent>();
        _agent.SetDestination(targetPos);
    }

    // DeskManager에서 호출할 메서드
    public void GetServed(Vector3 escapePos)
    {
        // 목적지를 감옥/탈출구로 설정
        SetDestination(escapePos);

        // 감옥 도착 후 처리 (옵션)
        StartCoroutine(_ArrivedAtCell());
    }

    private IEnumerator _ArrivedAtCell()
    {
        // 목적지 근처에 도착할 때까지 대기
        while (_agent.pathPending || _agent.remainingDistance > Constants.POINTONE)
        {
            yield return null;
        }
        // 도착하면 AI를 끄고 정지 상태 애니메이션으로 변경
        _agent.isStopped = true;
        _agent.enabled = false;
    }

    public void MoveToPrison(Vector3 targetPos, bool isFinalUpgradeDone)
    {
        _agent.SetDestination(targetPos);
        
        // 업그레이드 전이라면 일정 시간 후 사라지게 함
        if (!isFinalUpgradeDone)
        {
            StartCoroutine(_DisappearAfterArrival());
        }
    }

    private IEnumerator _DisappearAfterArrival()
    {
        // 목적지에 거의 도착할 때까지 대기
        while (_agent.remainingDistance > Constants.POINTTWO)
        {
            yield return null;
        }

        yield return _waitDuration; // 감옥 안에서 2초 정도 대기 후
    }
}