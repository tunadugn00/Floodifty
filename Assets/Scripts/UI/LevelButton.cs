using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [Header("UI Elemants")]
    public  GameObject highlightEffect;
    public GameObject levelCompleted;
    public GameObject levelUnCompleted;
    public TextMeshProUGUI levelText;
    public GameObject filledStars;
    public GameObject emptyStars;
    public Image[] filledStarImages;

    [Header("Line")]
    public GameObject lineOn;

    private int _levelIndex;
    private bool _isInteractable;


    public void Setup (int levelIndex, bool isBeaten, bool isNextPlayable)
    {
        _levelIndex = levelIndex;
        levelText.text = _levelIndex.ToString();

        bool isLocked = !isBeaten && !isNextPlayable;
        _isInteractable = !isLocked;

        //Set level button
        levelCompleted.SetActive(isBeaten);
        levelUnCompleted.SetActive(!isBeaten);

        //Highlight
        highlightEffect.SetActive(isNextPlayable);

        //Star
        filledStars.SetActive(isBeaten);
        emptyStars.SetActive(isBeaten);

        if (isBeaten)
        {
            int starCount = SaveSystem.GetLevelStars(_levelIndex);
            for(int i = 0; i < filledStarImages.Length; i++)
            {
                filledStarImages[i].gameObject.SetActive(i < starCount);
            }
        }

        //Lines
        if(lineOn != null)
        {
            lineOn.SetActive(!isLocked);
        }

        GetComponent<Button>().interactable = _isInteractable;
    }

    public void SetupComingSoon()
    {
        _isInteractable = false;

        levelText.gameObject.SetActive(false);
        levelCompleted.SetActive(false);
        levelUnCompleted.SetActive(true);
        highlightEffect.SetActive(false);
        filledStars.SetActive(false);
        emptyStars.SetActive(false);
        if(lineOn != null)
        {
            lineOn.SetActive(false);
        }
        GetComponent<Button>().interactable = false;
    }

    public void OnLevelSelected()
    {
        if (!_isInteractable) return;

        PlayerPrefs.SetInt("SelectedLevel", _levelIndex);
        SoundManager.Instance.PlayClick();
        SceneTransitionManager.Instance.LoadSceneWithAni("GameScene");
    }
}
