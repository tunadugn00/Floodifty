using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    public Transform contentParent;
    public GameObject levelMapChunkPrefab;

    public int totalLevel;
    public int levelsPerChunk = 15;

    public ScrollRect mainScrollRect;
    IEnumerator Start()
    {
        // Lấy totalLevel
        int unlockedLevel = SaveSystem.GetUnlockedLevel();
        int chunkCount = Mathf.CeilToInt((float)totalLevel / levelsPerChunk);

        RectTransform targetLevelRect = null;
        // tính index của level
        int targetChunkIndex = ( unlockedLevel - 1) / levelsPerChunk;
        int targetLevelInChunk = (unlockedLevel - 1) % levelsPerChunk;

        for (int i = 0; i < chunkCount; i++)
        {
            var chunkObj = Instantiate(levelMapChunkPrefab, contentParent);
            var chunk = chunkObj.GetComponent<LevelMapChunk>();
            int startLevel = (i * levelsPerChunk) + 1;
            chunk.SetupChunk(startLevel, unlockedLevel, totalLevel);

            // kiểm tra chunk có chứa level đó không
            if (i == targetChunkIndex)
            {
                if (targetLevelInChunk < chunk.buttonInThisChunk.Length)
                {
                    targetLevelRect = chunk.buttonInThisChunk[targetLevelInChunk].GetComponent<RectTransform>();
                }
            }
        }

        // Đợi 1 frame để Content Size Fitter tính toán
        yield return null;

        // tính vị trí scroll
        float viewportHeight = mainScrollRect.viewport.rect.height;
        float contentHeight = mainScrollRect.content.rect.height;
        float totalScrollableHeight = contentHeight - viewportHeight;

        //nếu content < viewport, không cần scroll
        if(totalScrollableHeight <= 0)
        {
            mainScrollRect.verticalNormalizedPosition = 0f;
            yield break;
        }

        //nếu không tìm thấy hoặc lv1 -> scroll lv1
        if(targetLevelRect == null)
        {
            var level1Chunk = contentParent.GetChild(0).GetComponent<LevelMapChunk>();
            targetLevelRect = level1Chunk.buttonInThisChunk[0].GetComponent<RectTransform>();
        }

        // lấy vị trí Y của nút ( so với đáy của content vì pivot Y = 0)
        float buttonY_relative = targetLevelRect.anchoredPosition.y
                                 + targetLevelRect.parent.GetComponent<RectTransform>().anchoredPosition.y;
        float targetContentY = buttonY_relative - (viewportHeight * 0.3f);
        float targetNormalizedPos = targetContentY / totalScrollableHeight;

        // set vị trí (và Clamp để đảm bảo nó nằm trong 0-1)
        mainScrollRect.verticalNormalizedPosition = Mathf.Clamp01(targetNormalizedPos);
    }


    public void BackButton()
    {
        SoundManager.Instance.PlayClick();
        SceneTransitionManager.Instance.LoadSceneWithAni("MainMenu");
    }
}
