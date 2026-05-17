using UnityEngine;

public class Singleton_Mono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<T>();

                if(_instance == null)
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    _instance = go.AddComponent<T>();
                }
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if(_instance == null)
        {
            _instance = this as T;
        }
        else if(_instance != this)
        {
            Debug.LogWarning($"[{typeof(T).Name}] 중복된 싱글톤 오브젝트가 발견되어 파괴되었습니다: {gameObject.name}");
            Destroy(gameObject);
        }
    }
}
