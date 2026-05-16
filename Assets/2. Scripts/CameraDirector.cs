using UnityEngine;
using Cinemachine; 
using System.Collections;

public class CameraDirector : MonoBehaviour
{
    public static CameraDirector Instance;

    [Header("Cinemachine Cameras")]
    public CinemachineVirtualCamera playerCam;  
    public CinemachineVirtualCamera upgradeCam; 

    private void Awake()
    {
        Instance = this;
    }

    public void ShowUpgradeArea(System.Action onComplete)
    {
        StartCoroutine(CameraSequence(onComplete));
    }

    IEnumerator CameraSequence(System.Action onComplete)
    {
        // 우선순위를 높여 카메라 전환 
        upgradeCam.Priority = 20; 

        // 카메라 전환시간 3초 대기
        yield return new WaitForSeconds(3.0f);

        // 우선순위를 낮춰 메인 카메라로 전환 
        upgradeCam.Priority = 5;

        // 카메라 전환시간 2초 대기 
        yield return new WaitForSeconds(2.0f);

        // 카메라 연출 종료 후 콜백 실행 
        onComplete?.Invoke();
    }
}