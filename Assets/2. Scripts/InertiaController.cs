using UnityEngine;

public class InertiaController : MonoBehaviour
{
    [SerializeField] private float _swayIntensity = 0.05f; // 휘어지는 강도 
    [SerializeField] private float _swayMaxAngle = 15.0f; // 최대 휘어짐 제한 
    [SerializeField] private float _returnSpeed = 5.0f; // 원래대로 돌아오는 복원 속도 

    private Vector3 _previousPosition; // 이전 프레임 위치 
    private Vector3 _moveVelocity; // 플레이어 실제 이동 속도 벡터 

    private PlayerStacker _playerStacker;

    private void Start()
    {
        _playerStacker = GetComponent<PlayerStacker>();

        _playerStacker.SetInertiaSettings(_swayIntensity, _swayMaxAngle, _returnSpeed);
    }

    private void Update()
    {
        if(_playerStacker.GetItemCount() <= Constants.ZERO_INTEGER) 
            return;

        // 1. 프레임간 위치 변화를 바탕으로 이동 속도 벡터 계산 (X, Z축만 사용)
        Vector3 currentPos = transform.position;
        Vector3 rawVelocity = (currentPos - _previousPosition) / Time.deltaTime;
        
        // Y축 높이 변화는 무시하고 수평 이동만 계산
        rawVelocity.y = Constants.ZERO_FLOAT; 

        // 속도 변화를 부드럽게 보간 (급격하게 튀는 현상 방지)
        _moveVelocity = Vector3.Lerp(_moveVelocity, rawVelocity, Time.deltaTime * _returnSpeed);
        _previousPosition = currentPos;

        Vector3 localVelocity = transform.InverseTransformDirection(_moveVelocity);

        // Anchor가 SmoothMove 코루틴이 진행중이라면 (Anchor가 뒤로 밀려나가거나 앞으로 오고 있는 중이면)
        if (_playerStacker.IsSortingAnchors)
        {
            // 관성 적용 X 
            _playerStacker.UpdateAllLayouts(Vector3.zero);
            return;
        }

        // 플레이어 뒤에 아이템들이 쌓여있고, Anchor가 SmoothMove 코루틴을 진행하고 있지 않으면(Anchor가 뒤로 밀려나가거나 앞으로 오고 있지 않으면) 
        if (_playerStacker.TotalCount > Constants.ZERO_INTEGER) 
        {
            _playerStacker.UpdateAllLayouts(localVelocity);
        }
    }
}
