using UnityEngine;
using Cinemachine; 
using System.Collections;

public enum CameraType
{
    Player,
    UpgradeArea,
}

public class CameraDirector : Singleton_Mono<CameraDirector>
{
    [Header("Cinemachine Cameras")]
    public CinemachineVirtualCamera playerCamera;
    public CinemachineVirtualCamera upgradeCamera; 
    private CinemachineVirtualCamera _targetCamera;

    [Header("Camera Settings")]
    [SerializeField] private int _activeCameraPriority = 20;
    [SerializeField] private int _inActiveCameraPriority = 5;
    [SerializeField] private float _activeCameraDuration = 3.0f;
    [SerializeField] private float _inActiveCameraDuration = 2.0f;

    private WaitForSeconds _activeDuration;
    private WaitForSeconds _inActiveDuration;

    protected override void Awake()
    {
        base.Awake();

        _activeDuration = new WaitForSeconds(_activeCameraDuration);
        _inActiveDuration = new WaitForSeconds(_inActiveCameraDuration);
    }

    public void ShowArea(CameraType cameraType)
    {
        _targetCamera = _GetCamera(cameraType);

        if(_targetCamera == null)
        {
            Debug.LogWarning($"해당하는 카메라를 찾을 수 없습니다.");
            return;
        }

        StartCoroutine(_CameraSequence(_targetCamera));
    }

    private CinemachineVirtualCamera _GetCamera(CameraType cameraType)
    {
        switch (cameraType)
        {
            case CameraType.Player : 
                return playerCamera;
            case CameraType.UpgradeArea : 
                return upgradeCamera;
            default : 
                return null;
        }
    }

    private IEnumerator _CameraSequence(CinemachineVirtualCamera targetCamera)
    {
        // 우선순위를 높여 카메라 전환 
        targetCamera.Priority = _activeCameraPriority;

        // 카메라 전환시간 3초 대기
        yield return _activeDuration;

        // 우선순위를 낮춰 메인 카메라로 전환 
        targetCamera.Priority = _inActiveCameraPriority;

        // 카메라 전환시간 2초 대기 
        yield return _inActiveDuration;
    }
}