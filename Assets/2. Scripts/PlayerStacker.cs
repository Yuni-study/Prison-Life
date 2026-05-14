using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerStacker : MonoBehaviour
{
    [Header("Stack Settings")]
    public Transform stackPoint;        // 석탄이 쌓이기 시작할 위치
    public int maxCapacity = 10;        // 최대 소지량
    public float stackHeightOffset = 0.25f; // 석탄 사이의 간격

    [Header("UI")]
    public GameObject maxTextUI;        // MAX 텍스트 오브젝트

    private Coroutine collectionCoroutine;

    private List<GameObject> collectedObjects = new List<GameObject>();

    public bool AddResource(GameObject prefab)
    {
        if (collectedObjects.Count >= maxCapacity)
        {
            ShowMaxText();
            return false;
        }

        GameObject newObj = Instantiate(prefab, stackPoint);
        
        // 석탄 프리팹에 "Coal" 태그가 붙어있는지 꼭 확인하세요!
        // 만약 코드에서 강제로 넣고 싶다면: newObj.tag = "Coal"; 

        // 물리 충돌 방지
        if(newObj.TryGetComponent<Collider>(out Collider col)) col.enabled = false;

        collectedObjects.Add(newObj);

        // 위치 계산 및 부드러운 이동
        float newY = (collectedObjects.Count - 1) * stackHeightOffset;
        Vector3 targetPos = new Vector3(0, newY, 0);
        StartCoroutine(SmoothMoveToStack(newObj, targetPos));

        return true;
    }

    private void ShowMaxText()
    {
        if (!maxTextUI.activeSelf)
        {
            maxTextUI.SetActive(true);
            Invoke("HideMaxText", 1f); // 1초 뒤 숨김
        }
    }

    private void HideMaxText()
    {
        maxTextUI.SetActive(false);
    }

    // 자원이 있는지 확인
    public bool HasResource() 
    {
        return collectedObjects.Count > 0;
    }

    // 석탄을 하나 버림 (변환기에 줄 때)
    public void RemoveResource()
    {
        if (collectedObjects.Count > 0)
        {
            int lastIndex = collectedObjects.Count - 1;
            GameObject toRemove = collectedObjects[lastIndex];
            
            // 리스트에서 먼저 제거하여 중복 참조 방지
            collectedObjects.RemoveAt(lastIndex);

            // 오브젝트가 존재할 때만 파괴
            if (toRemove != null)
            {
                Destroy(toRemove);
            }
        }
    }

    // 자원 추가 (석탄/수갑 공용)
    public bool AddSpecificResource(GameObject resourceObj)
    {
        if (collectedObjects.Count >= maxCapacity) return false;

        collectedObjects.Add(resourceObj);
        resourceObj.transform.SetParent(stackPoint);

        // 중요: Lerp 이동 전에 IsTrigger를 끄거나 레이어를 변경하여 
        // 자기 자신의 수집 영역에 다시 닿지 않게 해야 합니다.
        if(resourceObj.TryGetComponent<Collider>(out Collider col)) col.enabled = false;

        float newY = (collectedObjects.Count - 1) * stackHeightOffset;
        Vector3 targetLocalPos = new Vector3(0, newY, 0);

        StartCoroutine(SmoothMoveToStack(resourceObj, targetLocalPos));
        return true;
    }
    
    IEnumerator SmoothMoveToStack(GameObject obj, Vector3 targetPos)
    {
        float elapsed = 0f;
        float duration = 0.2f; 
        
        // 시작 시점에 이미 파괴되었는지 확인
        if (obj == null) yield break;

        Vector3 startPos = obj.transform.localPosition;
        Quaternion startRot = obj.transform.localRotation;

        while (elapsed < duration)
        {
            // 핵심: 루프 도중에도 오브젝트가 파괴되었는지 매 프레임 체크
            if (obj == null) yield break; 

            elapsed += Time.deltaTime;
            float percent = elapsed / duration;

            // 이동 및 회전 적용
            obj.transform.localPosition = Vector3.Lerp(startPos, targetPos, percent);
            obj.transform.localRotation = Quaternion.Lerp(startRot, Quaternion.identity, percent);
            
            yield return null;
        }

        // 마지막으로 한 번 더 체크 후 최종 위치 고정
        if (obj != null)
        {
            obj.transform.localPosition = targetPos;
            obj.transform.localRotation = Quaternion.identity;
        }
    }

    // 석탄만 가지고 있는지 확인하는 함수 (변환기용)
    public bool HasCoal()
    {
        for (int i = collectedObjects.Count - 1; i >= 0; i--)
        {
            if (collectedObjects[i] != null && collectedObjects[i].CompareTag("Coal"))
                return true;
        }
        return false;
    }

    // 석탄만 제거하는 함수 (변환기용)
    public void RemoveCoal()
    {
        for (int i = collectedObjects.Count - 1; i >= 0; i--)
        {
            if (collectedObjects[i] != null && collectedObjects[i].CompareTag("Coal"))
            {
                GameObject toRemove = collectedObjects[i];
                collectedObjects.RemoveAt(i);
                Destroy(toRemove);
                ReorderStack(); // 중간이 빠졌으니 정렬
                return;
            }
        }
    }

    // 중간 자원이 빠졌을 때 나머지 자원들을 아래로 밀착시키는 기능
    private void ReorderStack()
    {
        for (int i = 0; i < collectedObjects.Count; i++)
        {
            float newY = i * stackHeightOffset;
            Vector3 targetPos = new Vector3(0, newY, 0);
            StartCoroutine(SmoothMoveToStack(collectedObjects[i], targetPos));
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // 1. OutputArea 태그를 가진 영역 안에 머무는 동안
        if (other.CompareTag("OutputArea"))
        {
            // Debug.Log("OutputArea 안에 있음...");
            ResourceConverter converter = other.GetComponentInParent<ResourceConverter>();
            
            if (converter != null)
            {
                Debug.Log("ResourceConverter 발견!");
                // 아직 수집 코루틴이 돌고 있지 않다면 시작
                if (collectionCoroutine == null)
                {
                    collectionCoroutine = StartCoroutine(CollectHandcuffsRoutine(converter));
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 영역을 완전히 벗어나면 코루틴 중단
        if (other.CompareTag("OutputArea"))
        {
            if (collectionCoroutine != null)
            {
                StopCoroutine(collectionCoroutine);
                collectionCoroutine = null;
            }
        }
    }

    private IEnumerator CollectHandcuffsRoutine(ResourceConverter converter)
    {
        while (true)
        {
            Debug.Log("수집 시도 중...");

            // 1. 플레이어 소지량이 꽉 찼는지 확인
            if (collectedObjects.Count < maxCapacity)
            {
                // 2. 변환기에서 수갑을 하나 요청
                GameObject handcuff = converter.TakeHandcuff();

                if (handcuff != null)
                {
                    // 3. 내 뒤에 쌓기
                    AddSpecificResource(handcuff);
                    
                    // 4. 수집 간격 (0.1~0.2초 정도가 적당합니다)
                    yield return new WaitForSeconds(0.15f);
                }
                else
                {
                    // 변환기에 수갑이 없으면 잠시 대기
                    yield return new WaitForSeconds(0.2f);
                }
            }
            else
            {
                // 꽉 찼으면 MAX 표시 (기존 함수 활용)
                ShowMaxText();
                yield return new WaitForSeconds(1.0f);
            }
        }
    }
}