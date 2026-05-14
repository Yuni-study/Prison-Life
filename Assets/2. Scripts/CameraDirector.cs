using UnityEngine;
using Cinemachine; // 'Unity.'을 빼고 작성해 보세요.
using System.Collections;

public class CameraDirector : MonoBehaviour
{
    public static CameraDirector Instance;

    [Header("Cinemachine Cameras")]
    // 이름이 CinemachineVirtualCamera 인지 확인해 주세요.
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
        // 1. 업그레이드 카메라의 우선순위를 높여서 전환 시작
        // (기존 플레이어 카메라의 Priority가 10이라고 가정)
        upgradeCam.Priority = 20; 

        // 2. 카메라가 이동(Blend)하는 동안 대기
        // Cinemachine Brain에 설정된 Default Blend 시간만큼 기다립니다.
        yield return new WaitForSeconds(2.0f);

        // 3. 목적지에서 잠시 머무름
        yield return new WaitForSeconds(1.0f);

        // 4. 다시 우선순위를 낮춰서 플레이어 카메라로 복귀
        upgradeCam.Priority = 5;

        // 5. 복귀하는 시간 대기
        yield return new WaitForSeconds(2.0f);

        // 6. 모든 연출 종료 후 콜백 실행
        onComplete?.Invoke();
    }
}