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
        int unlockedLevel = SaveSystem.GetUnlockedLevel();
        int chunkCount = Mathf.CeilToInt((float)totalLevel / levelsPerChunk);

        RectTransform targetLevelRect = null;
        int targetChunkIndex = ( unlockedLevel - 1) / levelsPerChunk;
        int targetLevelInChunk = (unlockedLevel - 1) % levelsPerChunk;

        for (int i = 0; i < chunkCount; i++)
        {
            var chunkObj = Instantiate(levelMapChunkPrefab, contentParent);
            var chunk = chunkObj.GetComponent<LevelMapChunk>();
            int startLevel = (i * levelsPerChunk) + 1;
            chunk.SetupChunk(startLevel, unlockedLevel, totalLevel);

            if (i == targetChunkIndex)
            {
                if (targetLevelInChunk < chunk.buttonInThisChunk.Length)
                {
                    targetLevelRect = chunk.buttonInThisChunk[targetLevelInChunk].GetComponent<RectTransform>();
                }
            }
        }

        Canvas.ForceUpdateCanvases();
        yield return new WaitForEndOfFrame(); 
        if (targetLevelRect == null)
        {
            var level1Chunk = contentParent.GetChild(0).GetComponent<LevelMapChunk>();
            targetLevelRect = level1Chunk.buttonInThisChunk[0].GetComponent<RectTransform>();
        }

        Vector3 targetPosInContent = mainScrollRect.content.InverseTransformPoint(targetLevelRect.position);
        float viewportHeight = mainScrollRect.viewport.rect.height;

        float targetY = -targetPosInContent.y - (viewportHeight * 0.3f);

        Vector2 contentPos = mainScrollRect.content.anchoredPosition;
        contentPos.y = targetY;
        mainScrollRect.content.anchoredPosition = contentPos;

        Canvas.ForceUpdateCanvases();
        mainScrollRect.verticalNormalizedPosition = Mathf.Clamp01(mainScrollRect.verticalNormalizedPosition);
    }


    public void BackButton()
    {
        SoundManager.Instance.PlayClick();
        SceneTransitionManager.Instance.LoadSceneWithAni("MainMenu");
    }
}
