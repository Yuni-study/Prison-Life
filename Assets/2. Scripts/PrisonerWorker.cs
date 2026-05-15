using UnityEngine;

public class PrisonerWorker : MonoBehaviour
{
    [Header("Movement Points")]
    public Transform startPoint;
    public Transform endPoint;
    public float moveSpeed = 2f;

    [Header("Mining Settings")]
    public ResourceConverter targetConverter;
    public float mineCooldown = 1f;
    private float lastMineTime;

    private Vector3 offset; // 시작 지점으로부터의 상대적 거리
    private Vector3 currentTargetPos;
    private bool movingForward = true;

    void Start()
    {
        // 1. 게임 시작 시, 전체 경로의 시작점(startPoint)과 
        //    현재 자신의 위치 사이의 거리(오프셋)를 저장합니다.
        offset = transform.position - startPoint.position;
        
        // 첫 번째 목적지 설정
        UpdateTargetDestination();
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        // 2. 목적지로 이동 (오프셋이 포함된 좌표로 이동)
        transform.position = Vector3.MoveTowards(transform.position, currentTargetPos, moveSpeed * Time.deltaTime);
        
        // 회전 처리
        Vector3 direction = (currentTargetPos - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.15f);
        }

        // 목적지 도착 시 방향 전환
        if (Vector3.Distance(transform.position, currentTargetPos) < 0.1f)
        {
            movingForward = !movingForward;
            UpdateTargetDestination();
        }
    }

    private void UpdateTargetDestination()
    {
        // 3. 목적지를 계산할 때 기본 Point 위치에 자신의 오프셋을 더함
        // 이렇게 하면 3명이 가로 간격을 유지하며 평행하게 이동합니다.
        if (movingForward)
            currentTargetPos = endPoint.position + offset;
        else
            currentTargetPos = startPoint.position + offset;
    }

    private void OnTriggerStay(Collider other)
    {
        // 1. 자원 노드인지 확인
        if (other.CompareTag("ResourceNode"))
        {
            // 2. 쿨타임 확인
            if (Time.time >= lastMineTime + mineCooldown)
            {
                ResourceNode node = other.GetComponent<ResourceNode>();
                
                if (node != null)
                {
                    // 3. 노드의 채굴 함수 실행 (성공 시 true 반환)
                    bool success = node.ExecuteMining();
                    
                    if (success)
                    {
                        // 4. 변환기에 자원 추가
                        targetConverter.ReceiveResourceFromWorker();
                        lastMineTime = Time.time;
                        
                        Debug.Log(gameObject.name + "가 자원을 캐서 없앴습니다!");
                    }
                }
            }
        }
    }
}