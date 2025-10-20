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
        // Lấy chiều rộng của màn hình
        float panelWidth = (transitionMask.transform as RectTransform).rect.width;
        CanvasGroup maskGroup = transitionMask.GetComponent<CanvasGroup>();
        if(maskGroup ==  null)
        {
            maskGroup = transitionMask.gameObject.AddComponent<CanvasGroup>();
        }
        //Transition IN
        transitionMask.gameObject.SetActive(true);
        maskGroup.alpha = 0f;
        transitionMask.anchoredPosition = new Vector2(-panelWidth, 0);


        Sequence inSeq = DOTween.Sequence();
        inSeq.Join(transitionMask.DOAnchorPosX(0, swipeDuration).SetEase(Ease.OutCubic));
        inSeq.Join(maskGroup.DOFade(1, swipeDuration * 0.6f).SetEase(Ease.Linear));
        yield return inSeq.WaitForCompletion();

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
            loadingText.text = "Loading";
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        //Loading... text
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
        Sequence outSeq = DOTween.Sequence();
        outSeq.Join(transitionMask.DOAnchorPosX(panelWidth, swipeDuration).SetEase(Ease.InOutSine));
        outSeq.Join(maskGroup.DOFade(0, swipeDuration * 0.8f).SetEase(Ease.InOutSine));
        yield return outSeq.WaitForCompletion();

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(false);
        }
        transitionMask.gameObject.SetActive(false);
    }
}
