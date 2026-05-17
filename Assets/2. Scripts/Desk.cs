using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Desk : MonoBehaviour
{
    [Header("References")]
    public PrisonerManager prisonerManager;
    public Transform deskStackPoint;
    public Transform moneyPoint; 
    public GameObject moneyPrefab;

    [Header("Settings")]
    public float transferInterval = Constants.POINTZEROFIVE;
    public float stackHeightOffset = Constants.POINTTWO;
    public float moneyStackHeightOffset = Constants.POINTONE;

    [SerializeField] private float _moveToDeskDuration = Constants.POINTONE;
    [SerializeField] private float _moneySpawnDelay = Constants.POINTTWO;
    [SerializeField] private float _serveInterval = Constants.POINTONEFIVE;

    [Header("State")]
    public List<GameObject> handcuffsOnDesk = new List<GameObject>();
    public List<GameObject> moneyOnDesk = new List<GameObject>();
    private List<PlayerStacker> _stackersInRange = new List<PlayerStacker>();
    
    private Prisoner _targetPrisoner;
    private bool _isServing = false; // 죄수에게 수갑을 주는 루틴 중복 방지
    private bool _isTransferring = false; // 캐릭터에게서 수갑을 뺏어오는 루틴 중복 방지

    private WaitForSeconds _waitDuration1;
    private WaitForSeconds _waitDuration2;

    private void Awake()
    {
        _waitDuration1 = new WaitForSeconds(Constants.POINTTWO);
        _waitDuration2 = new WaitForSeconds(Constants.POINTONEFIVE);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Staff"))
        {
            if(other.TryGetComponent<PlayerStacker>(out PlayerStacker playerStacker))
            {
                if (!_stackersInRange.Contains(playerStacker))
                {
                    _stackersInRange.Add(playerStacker);

                    if (!_isTransferring)
                    {
                        // 수갑 수거 코루틴 
                        StartCoroutine(_TransferHandcuffsRoutine());
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Staff"))
        {
            if(other.TryGetComponent<PlayerStacker>(out PlayerStacker playerStacker))
            {
                _stackersInRange.Remove(playerStacker);
            }
        }
    }

    // --- 수거 루틴: 캐릭터(플레이어/직원)에게서 수갑을 데스크로 가져옴 ---
    private IEnumerator _TransferHandcuffsRoutine()
    {
        _isTransferring = true;
        while (_stackersInRange.Count > Constants.ZERO_INTEGER) 
        {
            bool foundItem = false;
            foreach (var stacker in _stackersInRange)
            {
                if (stacker == null) continue;
                
                GameObject handcuff = stacker.PopSpecificItem("Handcuff");
                if (handcuff != null)
                {
                    handcuffsOnDesk.Add(handcuff);
                    handcuff.transform.SetParent(deskStackPoint);
                    
                    float newY = (handcuffsOnDesk.Count - 1) * stackHeightOffset;
                    StartCoroutine(_MoveToDesk(handcuff, new Vector3(Constants.ZERO_FLOAT, newY, Constants.ZERO_FLOAT))); 
                    
                    foundItem = true;
                    yield return new WaitForSeconds(transferInterval);
                    break; 
                }
            }
            if (!foundItem) yield return null;
        }
        _isTransferring = false;
    }

    // --- 서비스 루틴: 데스크에 있는 수갑을 죄수에게 전달 ---
    public void SetTargetPrisoner(Prisoner p)
    {
        _targetPrisoner = p;
        if (!_isServing) StartCoroutine(_ServeRoutine());
    }

    private IEnumerator _ServeRoutine()
    {
        _isServing = true;
        while (_targetPrisoner != null)
        {
            if (handcuffsOnDesk.Count > Constants.ZERO_INTEGER) 
            {
                GameObject handcuff = handcuffsOnDesk[Constants.ZERO_INTEGER];
                handcuffsOnDesk.RemoveAt(Constants.ZERO_INTEGER);
                
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlaySFX(SoundManager.Instance.prisonerGetClip);

                if(handcuff != null)
                {
                    Destroy(handcuff);
                }

                _targetPrisoner.needHandcuffs--; // 죄수의 필요 수치 감소
                _ReorderDeskStack();

                if (_targetPrisoner.needHandcuffs <= Constants.ZERO_INTEGER)
                {
                    yield return _waitDuration1;
                    _SpawnMoney();
                    prisonerManager.DismissPrisoner();
                    _targetPrisoner = null;
                }
                yield return _waitDuration2;
            }
            else yield return null;
        }
        _isServing = false;
    }

    // --- 공용 유틸리티 함수 ---
    private void _SpawnMoney()
    {
        if (moneyPrefab == null) return;
        GameObject money = Instantiate(moneyPrefab, moneyPoint);
        money.transform.localPosition = new Vector3(Constants.ZERO_FLOAT, moneyOnDesk.Count * moneyStackHeightOffset, Constants.ZERO_FLOAT); 
        moneyOnDesk.Add(money);
        money.tag = "Money";
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySFX(SoundManager.Instance.moneySpawnClip);
    }

    private void _ReorderDeskStack()
    {
        for (int i = 0; i < handcuffsOnDesk.Count; i++)
        {
            if (handcuffsOnDesk[i] != null)
                handcuffsOnDesk[i].transform.localPosition = new Vector3(Constants.ZERO_FLOAT, i * stackHeightOffset, Constants.ZERO_FLOAT);
        }
    }

    private IEnumerator _MoveToDesk(GameObject obj, Vector3 targetPos)
    {
        float elapsed = Constants.ZERO_FLOAT;
        Vector3 startPos = obj.transform.localPosition;
        while (elapsed < _moveToDeskDuration)
        {
            if (obj == null) yield break;
            elapsed += Time.deltaTime;
            obj.transform.localPosition = Vector3.Lerp(startPos, targetPos, elapsed / Constants.POINTONE);
            yield return null;
        }
        if (obj != null) obj.transform.localPosition = targetPos;
    }
}