using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _moveSpeedModifier = 1.0f;
    [SerializeField] private float _rotationSpeed = 720f;
    [SerializeField] private float _rotationSpeedModifier = 1.0f;
    private Vector2 _inputVector; 
    private Vector3 _moveDirection;
    private Vector3 _lookDirection;
    private Rigidbody _rigid;
    private bool _isMoving;

    [Header("Delegate")]
    public Action<bool> OnMovementStateChanged; 

    [Header("Joystick Reference")]
    [SerializeField] private FloatingJoystick joystick; // 조이스틱 연결용

    private Vector3 moveInput;


    private void Awake()
    {
        _rigid = GetComponent<Rigidbody>();

        _rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // X, Z축 회전 고정

        // 만약 인스펙터에서 할당을 깜빡했다면 자동으로 찾아주는 방어 코드
        if (joystick == null)
            joystick = FindObjectOfType<FloatingJoystick>();
    }

    void Update()
    {
        _CheckInput();
    }

    void FixedUpdate()
    {
        // Move();
        _Move();
        _Rotation();
    }

    private void _CheckInput()
    {
        _inputVector = new Vector2(joystick.Horizontal, joystick.Vertical);
        Debug.Log($"_inputVector = {_inputVector}");
        _CheckMovementState();
    }

    private void _CheckMovementState()
    {
        bool currentlyMoving = _inputVector.sqrMagnitude > 0.01f; 
        if(_isMoving != currentlyMoving)
        {
            _isMoving = currentlyMoving;
            OnMovementStateChanged?.Invoke(_isMoving); // 현재 이동상태가 변경되었을 때 이벤트(가만히 있으면 이동하라는 UI 활성화) 호출 
        }
    }

    private void _Move()
    {
        // inputVector가 (0,0)일 때는 이동 X 
        if(!_isMoving) return;

        _moveDirection = new Vector3(_inputVector.x, 0.0f, _inputVector.y).normalized; 
        _rigid.MovePosition(_rigid.position + _moveDirection * (_moveSpeed * _moveSpeedModifier) * Time.fixedDeltaTime); // 목표지점까지 이동 
    }

    private void _Rotation()
    {
        // inputVector가 (0,0)일 때는 회전 X 
        if(!_isMoving) return;

        _lookDirection = new Vector3(_inputVector.x, 0.0f, _inputVector.y);
        
        Quaternion lookRotation = Quaternion.LookRotation(_lookDirection);
        _rigid.rotation = Quaternion.RotateTowards(_rigid.rotation, lookRotation, (_rotationSpeed * _rotationSpeedModifier) * Time.fixedDeltaTime); // lookRotation 방향으로 부드럽게 회전 
    }

    private void Move()
    {
        // 입력값이 일정 수준 이상일 때만 움직이도록 설정 (데드존)
        if (moveInput.magnitude >= 0.1f)
        {
            _rigid.MovePosition(_rigid.position + moveInput * _moveSpeed * Time.fixedDeltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            _rigid.rotation = Quaternion.RotateTowards(_rigid.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
        }
    }

    // 안내 UI(가만히 있을 때 나타나는 UI) 출력 
    private void HandleIdleUI()
    {
        // 플레이어가 가만히 있는지 확인하는 로직이 들어갈 자리입니다.
        // moveInput.magnitude가 0이면 가만히 있는 상태겠죠?
    }
}