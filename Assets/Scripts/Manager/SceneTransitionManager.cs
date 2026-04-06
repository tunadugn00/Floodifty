using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    [SerializeField] private RectTransform transitionMask;
    [SerializeField] private float swipeDuration = 0.6f;
    [SerializeField] private GameObject loadingSpinner;

    private void Awake()
    {
        if (Instance == null)
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
        if (maskGroup == null)
        {
            maskGroup = transitionMask.gameObject.AddComponent<CanvasGroup>();
        }

        transitionMask.gameObject.SetActive(true);
        maskGroup.alpha = 0f;
        yield return maskGroup.DOFade(1, swipeDuration).SetEase(Ease.InOutSine).WaitForCompletion();

        if (loadingSpinner != null) loadingSpinner.SetActive(true);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
        yield return new WaitUntil(() => asyncLoad.isDone);
        yield return maskGroup.DOFade(0, swipeDuration).SetEase(Ease.InOutSine).WaitForCompletion();

        if (loadingSpinner != null) loadingSpinner.SetActive(false);
        transitionMask.gameObject.SetActive(false);
    }
}