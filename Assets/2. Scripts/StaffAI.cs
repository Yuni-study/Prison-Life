using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class StaffAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private PlayerStacker myStacker;

    [Header("Target Points")]
    public Transform outputArea; // ResourceConverter의 수갑 나오는 곳
    public Transform deskArea;   // DeskManager의 수갑 놓는 곳

    private bool isWorking = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        myStacker = GetComponent<PlayerStacker>();
        
        // 태그를 Staff로 설정 (데스크에서 플레이어와 구분하기 위함)
        gameObject.tag = "Staff"; 
        
        StartCoroutine(StaffLoop());
    }

    IEnumerator StaffLoop()
    {
        while (true)
        {
            // 1. 등에 수갑이 없고(0개), 컨버터에 수갑이 있으면 가지러 감
            if (myStacker.GetItemCount() == 0 && ResourceConverter.Instance.HasHandcuffs())
            {
                yield return StartCoroutine(MoveToTarget(outputArea.position));
                CollectHandcuffs();
            }
            // 2. 등에 아이템이 하나라도 있으면 데스크로 배달함
            else if (myStacker.GetItemCount() > 0)
            {
                // // 1. 데스크로 이동
                // yield return StartCoroutine(MoveToTarget(deskArea.position));
                
                // // 2. [중요] 데스크 영역 안에서 수갑이 0개가 될 때까지 기다림
                // // DeskManager가 스태프의 등에 있는 수갑을 하나씩 빼갈 것입니다.
                // float timeout = 0f;
                // while (myStacker.GetItemCount() > 0 && timeout < 5f) // 최대 5초 대기 (무한루프 방지)
                // {
                //     timeout += Time.deltaTime;
                //     yield return null; 
                // }
                
                // Debug.Log("스태프: 수갑 전달 완료. 다시 수집하러 갑니다.");

                yield return StartCoroutine(MoveToTarget(deskArea.position));
    
                // 내 등에 수갑이 0개가 될 때까지 영역에서 이탈하지 않고 기다림
                // 이제 DeskManager가 리스트를 돌며 내 수갑을 다 빼갈 때까지 직원은 안전하게 대기합니다.
                while (myStacker.GetSpecificItemCount("Handcuff") > 0)
                {
                    yield return new WaitForSeconds(0.2f);
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator MoveToTarget(Vector3 target)
    {
        agent.SetDestination(target);
        while (agent.pathPending || agent.remainingDistance > 0.5f)
        {
            yield return null;
        }
    }

    void CollectHandcuffs()
    {
        // 내 가방이 찰 때까지(IsFull) 혹은 컨버터 수갑이 다 떨어질 때까지 루프
        while (!myStacker.IsFull() && ResourceConverter.Instance.HasHandcuffs())
        {
            GameObject handcuff = ResourceConverter.Instance.TakeHandcuff();
            if (handcuff != null)
            {
                // PlayerStacker의 AddItemToList를 호출하면 자동으로 등에 쌓입니다.
                myStacker.AddItemToList(handcuff, "Handcuff");
                
                // 효과음 재생 (옵션)
                if(SoundManager.Instance != null)
                    SoundManager.Instance.PlaySFX(SoundManager.Instance.collectClip);
            }
        }
    }
}