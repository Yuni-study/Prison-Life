using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : Singleton_Mono<UIManager>
{
    [Header("Money UI")]
    public TextMeshProUGUI moneyText;

    [Header("Sound UI")]
    public Image soundIcon;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    public System.Action<int> OnMoneyChanged;

    private void OnEnable()
    {
        OnMoneyChanged += UpdateMoneyUI;
    }

    private void OnDisable()
    {
        OnMoneyChanged -= UpdateMoneyUI;
    }

    // 돈 업데이트 (PlayerStacker나 다른 곳에서 호출)
    public void UpdateMoneyUI(int amount)
    {
        if (moneyText != null)
            moneyText.text = amount.ToString("N0"); // 1,000 단위 콤마 표시
    }

    // 사운드 버튼 클릭 시 호출 (UI Button의 OnClick에 연결)
    public void OnSoundButtonClicked()
    {
        SoundManager.Instance.ToggleMute();
        
        // 아이콘 변경
        if (SoundManager.Instance.IsMuted())
            soundIcon.sprite = soundOffSprite;
        else
            soundIcon.sprite = soundOnSprite;
    }
}