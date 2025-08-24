using UnityEngine;

public class LevelSelectManager : MonoBehaviour
{
    public Transform contentParent; // ScrollView/Content
    public GameObject levelButtonPrefabs;
    public int totalLevel;

  

    void Start()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        for (int i = 1; i <= totalLevel ; i++)
        {
            var buttonObj = Instantiate(levelButtonPrefabs, contentParent);
            var btn = buttonObj.GetComponent<LevelButton>();
            bool unlocked = i <= unlockedLevel;
            btn.Setup(i, unlocked);
        }

        
    }
}
