using UnityEngine;
using TMPro; // TextMeshPro 사용을 위해 필요

public class ZoneUI : MonoBehaviour
{
    public TextMeshProUGUI costText; // Canvas 내의 텍스트 연결
    public string prefix = "COST: ";

    public void UpdateUI(int current, int total)
    {
        if (costText != null)
        {
            // 예: "COST: 500 / 1000" 형태로 표시
            costText.text = $"{prefix}{current} / {total}";
        }
    }

    public void UpdateUI(string message, int remainingCost)
    {
        if (costText != null)
        {
            // 남은 금액이 0보다 클 때만 금액을 표시하고, 0이면 메시지만 표시
            if (remainingCost > 0)
                costText.text = $"{message}: {remainingCost}";
            else
                costText.text = message;
        }
    }

    public void SetMax()
    {
        if (costText != null) costText.text = "MAX";
    }
}