using UnityEngine;

public class MoneyArea : MonoBehaviour
{
    private bool hasGuided = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasGuided && other.CompareTag("Player"))
        {
            // 실제 서비스 시에는 돈이 생성되었는지 체크하는 로직이 있으면 좋습니다.
            hasGuided = true;
            
            // 플레이어 조작 멈춤 (필요 시)
            // other.GetComponent<PlayerMovement>().SetCanMove(false);

            CameraDirector.Instance.ShowArea(CameraType.UpgradeArea);
        }
    }
}