using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourceConverter : Singleton_Mono<ResourceConverter>
{
    [Header("Speed Settings")]
    public float inputSpeed = 0.05f;  // 자원을 플레이어에게서 가져오는 간격 (초)
    public float outputSpeed = 0.08f; // 수갑이 생성되는 간격 (초)
    public int maxOutputStack = 30;   // 보관 용량도 조금 늘려줍니다.

    [Header("Settings")]
    public GameObject handcuffPrefab;
    public Transform outputPoint;
    public float convertTime = 0.15f;

    [Header("State")]
    private List<GameObject> _producedHandcuffs = new List<GameObject>();
    private Coroutine _inputRoutine;
    private int _pendingConversions = 0; // 현재 변환 대기 중인 석탄 개수

    // 대기시간 변수 
    private WaitForSeconds _inputWait;
    private WaitForSeconds _convertWait;

    protected override void Awake()
    {
        base.Awake();

        _inputWait = new WaitForSeconds(inputSpeed);
        _convertWait = new WaitForSeconds(inputSpeed * 2f);
    }

    public void SetPlayerInInput(PlayerStacker playerStacker)
    {
        if (playerStacker != null)
        {
            if (_inputRoutine != null) StopCoroutine(_inputRoutine);
            _inputRoutine = StartCoroutine(_ProcessInput(playerStacker));
        }
        else
        {
            if (_inputRoutine != null)
            {
                StopCoroutine(_inputRoutine);
                _inputRoutine = null;
            }
        }
    }

    // 자원을 빠르게 빨아들이는 코루틴
    private IEnumerator _ProcessInput(PlayerStacker playerStacker)
    {
        while (true)
        {
            // 출력 공간이 있고 플레이어가 석탄을 가지고 있다면
            if (_producedHandcuffs.Count + _pendingConversions < maxOutputStack && playerStacker.HasCoal())
            {
                playerStacker.RemoveCoal();       // 플레이어 뒤에서 제거
                _pendingConversions++;      // 변환 대기열 추가
                
                // 변환 시작 (비동기로 실행하여 입력을 막지 않음)
                StartCoroutine(_ConvertLogic()); 
                
                yield return _inputWait; // 투입 속도
            }
            else
            {
                yield return null; // 조건 안 맞으면 한 프레임 대기
            }
        }
    }

    // 내부 변환 및 수갑 생성 코루틴
    private IEnumerator _ConvertLogic()
    {
        // 실제 변환 공정 시간 (필요 시 조절)
        yield return _convertWait; 
        
        _SpawnHandcuff();
        _pendingConversions--;
    }

    private void _SpawnHandcuff()
    {
        GameObject newHandcuff = Instantiate(handcuffPrefab, outputPoint);
        SoundManager.Instance.PlaySFX(SoundManager.Instance.outputClip);
        newHandcuff.tag = "Handcuff";

        // 쌓이는 위치 계산
        float yOffset = _producedHandcuffs.Count * 0.2f;
        newHandcuff.transform.localPosition = new Vector3(0, yOffset, 0);
        newHandcuff.transform.localRotation = Quaternion.identity;

        _producedHandcuffs.Add(newHandcuff);
    }

    public GameObject TakeHandcuff()
    {
        if (_producedHandcuffs.Count > 0)
        {
            int lastIndex = _producedHandcuffs.Count - 1;
            GameObject lastHandcuff = _producedHandcuffs[lastIndex];
            _producedHandcuffs.RemoveAt(lastIndex);
            return lastHandcuff;
        }
        return null;
    }

    public void ReceiveResourceFromWorker()
    {
        if (_producedHandcuffs.Count + _pendingConversions < maxOutputStack)
        {
            _pendingConversions++;
            StartCoroutine(_ConvertLogic());
        }
    }

    // 직원이 수갑을 가져갈 수 있는지 확인하는 메서드 
    public bool HasHandcuffs() => _producedHandcuffs.Count > 0;
}