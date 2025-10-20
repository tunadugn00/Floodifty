using UnityEngine;

public class LevelSelectManager : MonoBehaviour
{
    public Transform contentParent; // ScrollView/Content
    public GameObject levelButtonPrefabs;
    public int totalLevel;

  

    void Start()
    {
        int unlockedLevel = SaveSystem.GetUnlockedLevel();

        for (int i = 1; i <= totalLevel ; i++)
        {
            var buttonObj = Instantiate(levelButtonPrefabs, contentParent);
            var btn = buttonObj.GetComponent<LevelButton>();
            bool unlocked = i <= unlockedLevel;
            btn.Setup(i, unlocked);
        }

        
    }

    public void BackButton()
    {
        SoundManager.Instance.PlayClick();
        SceneTransitionManager.Instance.LoadSceneWithAni("MainMenu");
    }
}
