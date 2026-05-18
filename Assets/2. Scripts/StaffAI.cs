using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class StaffAI : MonoBehaviour
{
    private NavMeshAgent _agent;
    private PlayerStacker _myStacker;

    [Header("Target Points")]
    public Transform outputArea; // ResourceConverter의 수갑 나오는 곳
    public Transform deskArea;   // DeskManager의 수갑 놓는 곳

    private WaitForSeconds _waitDuration1;
    private WaitForSeconds _waitDuration2;

    private void Awake()
    {
        _waitDuration1 = new WaitForSeconds(Constants.POINTTWO);
        _waitDuration2 = new WaitForSeconds(Constants.POINTFIVE);
    }

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _myStacker = GetComponent<PlayerStacker>();
        
        // 태그를 Staff로 설정 (데스크에서 플레이어와 구분하기 위함)
        gameObject.tag = "Staff"; 
        
        StartCoroutine(_StaffLoop());
    }

    private IEnumerator _StaffLoop()
    {
        while (true)
        {
            // 1. 등에 수갑이 없고(0개), 컨버터에 수갑이 있으면 가지러 감
            if (_myStacker.GetItemCount() == Constants.ZERO_INTEGER && ResourceConverter.Instance.HasHandcuffs())
            {
                yield return StartCoroutine(_MoveToTarget(outputArea.position));
                _CollectHandcuffs();
            }

            // 2. 등에 아이템이 하나라도 있으면 데스크로 배달함
            else if (_myStacker.GetItemCount() > Constants.ZERO_INTEGER)
            {
                yield return StartCoroutine(_MoveToTarget(deskArea.position));
    
                // 내 등에 수갑이 0개가 될 때까지 영역에서 이탈하지 않고 기다림
                // 이제 DeskManager가 리스트를 돌며 내 수갑을 다 빼갈 때까지 직원은 안전하게 대기합니다.
                while (_myStacker.GetSpecificItemCount("Handcuff") > Constants.ZERO_INTEGER)
                {
                    yield return _waitDuration1;
                }
            }

            yield return _waitDuration2;
        }
    }

    private IEnumerator _MoveToTarget(Vector3 target)
    {
        _agent.SetDestination(target);
        while (_agent.pathPending || _agent.remainingDistance > Constants.POINTFIVE)
        {
            yield return null;
        }
    }

    private void _CollectHandcuffs()
    {
        // 내 가방이 찰 때까지(IsFull) 혹은 컨버터 수갑이 다 떨어질 때까지 루프
        while (!_myStacker.IsFull("Handcuff") && ResourceConverter.Instance.HasHandcuffs())
        {
            GameObject handcuff = ResourceConverter.Instance.TakeHandcuff();
            if (handcuff != null)
            {
                // PlayerStacker의 AddItemToList를 호출하면 자동으로 등에 쌓입니다.
                _myStacker.AddItemToList(handcuff, "Handcuff");
                
                // 효과음 재생 (옵션)
                if(SoundManager.Instance != null)
                    SoundManager.Instance.PlaySFX(SoundManager.Instance.collectClip);
            }
        }
    }
}