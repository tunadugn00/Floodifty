using DG.Tweening;
using System.Collections;
using UnityEngine;

public class StarDisplay : MonoBehaviour
{
    public GameObject[] stars;

    [Header("Animation Settings")]
    public float starScaleDuration = 0.5f;
    public float delayBetweenStars = 0.2f;
    public Ease starEaseType = Ease.OutBack;
    public void Show(int count)
    {
        StopAllCoroutines();
        StartCoroutine(ShowAnimated(count));
    }

    private IEnumerator ShowAnimated (int count)
    {
        //reset scale * về 0
        foreach (GameObject star in stars)
        {
            star.SetActive(false);
            star.transform.localScale = Vector3.zero;

        }

        // chạy animation
        for (int i = 0; i < stars.Length; i++)
        {
            if( i < count)
            {
                stars[i].SetActive(true);
                stars[i].transform.DOScale(1f, starScaleDuration)
                    .SetEase(starEaseType);

                //delay
                yield return new WaitForSeconds(delayBetweenStars); 
            }
            else
            {
                stars[i].SetActive(false);
            }
        }
    }
}
