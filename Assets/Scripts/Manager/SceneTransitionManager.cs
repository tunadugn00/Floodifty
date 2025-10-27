using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    [SerializeField] private RectTransform transitionMask;
    [SerializeField] private float swipeDuration = 0.6f;
    [SerializeField] private TextMeshProUGUI loadingText;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void LoadSceneWithAni(string sceneName)
    {
        StartCoroutine(DoSceneTransition(sceneName));
    }

    private IEnumerator DoSceneTransition(string sceneName)
    {
        CanvasGroup maskGroup = transitionMask.GetComponent<CanvasGroup>();
        if(maskGroup ==  null)
        {
            maskGroup = transitionMask.gameObject.AddComponent<CanvasGroup>();
        }
        //Transition IN
        transitionMask.gameObject.SetActive(true);
        maskGroup.alpha = 0f;

        yield return maskGroup.DOFade(1, swipeDuration).SetEase(Ease.InOutSine).WaitForCompletion();

        //Loading... text
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
            loadingText.text = "Loading";
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        float timer = 0f;
        float minimumLoadTime = 1.5f;

        while (asyncLoad.progress < 0.9f || timer < minimumLoadTime)
        {
            timer += Time.unscaledDeltaTime;
            if (loadingText != null)
            {
                int dotCount = Mathf.FloorToInt(timer * 2f) % 4;
                loadingText.text = "Loading" + new string('.', dotCount);
            }
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
        yield return new WaitUntil(() => asyncLoad.isDone);

        //Transition OUT
        yield return maskGroup.DOFade(0, swipeDuration).SetEase(Ease.InOutSine).WaitForCompletion();

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(false);
        }
        transitionMask.gameObject.SetActive(false);
    }
}
