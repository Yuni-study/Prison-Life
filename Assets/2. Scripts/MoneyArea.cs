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

            CameraDirector.Instance.ShowUpgradeArea(() => {
                // 가이드 완료 후 다시 조작 가능하게 함
                // other.GetComponent<PlayerMovement>().SetCanMove(true);
                Debug.Log("카메라 연출 끝!");
            });
        }
    }
}