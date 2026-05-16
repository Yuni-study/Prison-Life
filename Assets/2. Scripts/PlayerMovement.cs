using System;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Input System")]
    private PlayerInputActions _playerInputActions;
    private InputAction _inputAction;

    private void Awake()
    {
        _rigid = GetComponent<Rigidbody>();

        _rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // X, Z축 회전 고정

        _playerInputActions = new PlayerInputActions();
        _inputAction = _playerInputActions.Player.Move; 
    }

    private void OnEnable()
    {
        _inputAction.Enable();
    }

    private void OnDisable()
    {
        _inputAction.Disable();
    }

    void Update()
    {
        _CheckInput();
    }

    void FixedUpdate()
    {
        _Move();
        _Rotation();
    }

    private void _CheckInput()
    {
        _inputVector = _inputAction.ReadValue<Vector2>(); 
        // Debug.Log($"_inputVector = {_inputVector}");

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

    // 안내 UI(가만히 있을 때 나타나는 UI) 출력 
    private void HandleIdleUI()
    {
        // 플레이어가 가만히 있는지 확인하는 로직이 들어갈 자리입니다.
        // moveInput.magnitude가 0이면 가만히 있는 상태겠죠?
    }
}