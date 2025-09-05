using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public GameObject lockIcon;
    public GameObject starIcon;


    private int levelIndex;
    private bool unlocked;

    public void Setup (int level, bool isUnlocked)
    {
        levelIndex = level;
        unlocked = isUnlocked;
        levelText.text = (level).ToString();
        lockIcon.SetActive(!unlocked);
    }

    public void OnClick()
    {
        if (!unlocked) return;

        PlayerPrefs.SetInt("SelectedLevel", levelIndex);
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
}
