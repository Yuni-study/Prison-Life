using UnityEngine;

public class MoneyArea : MonoBehaviour
{
    private bool _hasGuided = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!_hasGuided && other.CompareTag("Player"))
        {
            // 실제 서비스 시에는 돈이 생성되었는지 체크하는 로직이 있으면 좋습니다.
            _hasGuided = true;

            CameraDirector.Instance.ShowArea(CameraType.UpgradeArea);
        }
    }
}