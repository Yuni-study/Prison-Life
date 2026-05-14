using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;

    [Header("Joystick Reference")]
    [SerializeField] private FloatingJoystick joystick; // 조이스틱 연결용

    private Rigidbody rb;
    private Vector3 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // 만약 인스펙터에서 할당을 깜빡했다면 자동으로 찾아주는 방어 코드
        if (joystick == null)
            joystick = FindObjectOfType<FloatingJoystick>();
    }

    void Update()
    {
        // 1. 조이스틱 입력 받기
        // 조이스틱의 Horizontal(가로), Vertical(세로) 값을 Vector3의 X와 Z에 대입합니다.
        float horizontal = joystick.Horizontal;
        float vertical = joystick.Vertical;

        moveInput = new Vector3(horizontal, 0f, vertical).normalized;

        // 2. 안내 UI 관련 로직 (나중에 추가)
        HandleIdleUI();
    }

    void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        // 입력값이 일정 수준 이상일 때만 움직이도록 설정 (데드존)
        if (moveInput.magnitude >= 0.1f)
        {
            rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void HandleIdleUI()
    {
        // 플레이어가 가만히 있는지 확인하는 로직이 들어갈 자리입니다.
        // moveInput.magnitude가 0이면 가만히 있는 상태겠죠?
    }
}