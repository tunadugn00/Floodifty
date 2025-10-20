using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public GameObject lockIcon;
    public StarDisplay starDisplay;

    private int levelIndex;
    private bool unlocked;

    public void Setup (int level, bool isUnlocked)
    {
        levelIndex = level;
        unlocked = isUnlocked;
        levelText.text = (level).ToString();
        lockIcon.SetActive(!unlocked);

        int stars = SaveSystem.GetLevelStars(level);
        starDisplay.Show(stars);
    }

    public void OnClick()
    {
        if (!unlocked) return;

        PlayerPrefs.SetInt("SelectedLevel", levelIndex);
        SoundManager.Instance.PlayClick();
        SceneTransitionManager.Instance.LoadSceneWithAni("GameScene");
    }
}
