using UnityEngine;

public class StarDisplay : MonoBehaviour
{
    public GameObject[] stars;

    public void Show(int count)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].SetActive(i < count);
        }
    }
}
