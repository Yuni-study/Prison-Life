using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip mineClip;        // 자원 캘 때
    public AudioClip inputClip;       // 컨버터에 넣을 때
    public AudioClip outputClip;     // 컨버터에서 나올 때
    public AudioClip collectClip;     // 수갑 가져올 때
    public AudioClip prisonerGetClip; // 죄수가 수갑 받을 때
    public AudioClip moneySpawnClip;  // 돈 생성될 때

    private bool isMuted = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 씬이 바뀌어도 파괴되지 않게 하려면 추가 (선택사항)
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) 
        {
            Debug.LogWarning("사운드 클립이 할당되지 않았습니다!");
            return;
        }

        if (isMuted) return;
        
        sfxSource.PlayOneShot(clip);
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        sfxSource.mute = isMuted;
    }

    public bool IsMuted() => isMuted;
}