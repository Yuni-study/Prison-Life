using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourceConverter : MonoBehaviour
{
    [Header("Settings")]
    public GameObject handcuffPrefab;
    public Transform outputPoint;
    public float convertTime = 0.5f;
    public int maxOutputStack = 10;

    [Header("State")]
    private List<GameObject> producedHandcuffs = new List<GameObject>();
    private bool isProcessing = false;

    // 플레이어를 기억하기 위한 변수
    private PlayerStacker playerInInput;

    // --- 외부(자식 트리거)에서 호출해줄 함수들 ---
    public void SetPlayerInInput(PlayerStacker player) => playerInInput = player;

    void Update()
    {
        // 입력 영역에 플레이어가 있고, 변환 중이 아니며, 출력 공간이 있을 때
        if (playerInInput != null && !isProcessing && producedHandcuffs.Count < maxOutputStack)
        {
            if (playerInInput.HasCoal())
            {
                StartCoroutine(ConvertProcess(playerInInput));
            }
        }
    }

    IEnumerator ConvertProcess(PlayerStacker player)
    {
        isProcessing = true;
        player.RemoveCoal();
        yield return new WaitForSeconds(convertTime);
        SpawnHandcuff();
        isProcessing = false;
    }

    void SpawnHandcuff()
    {
        GameObject newHandcuff = Instantiate(handcuffPrefab, outputPoint);
        newHandcuff.tag = "Handcuff"; // 수갑 태그 확인

        float yOffset = producedHandcuffs.Count * 0.2f;
        newHandcuff.transform.localPosition = new Vector3(0, yOffset, 0);
        newHandcuff.transform.localRotation = Quaternion.identity;

        producedHandcuffs.Add(newHandcuff);
    }

    public GameObject TakeHandcuff()
    {
        if (producedHandcuffs.Count > 0)
        {
            int lastIndex = producedHandcuffs.Count - 1;
            GameObject lastHandcuff = producedHandcuffs[lastIndex];
            producedHandcuffs.RemoveAt(lastIndex);
            return lastHandcuff;
        }
        return null;
    }
}