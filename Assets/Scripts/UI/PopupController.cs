using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class PopupController : MonoBehaviour
{

    public void ShowPopup(CanvasGroup panel, RectTransform content)
    {
        panel.gameObject.SetActive(true);

        // reset trạng thái ban đầu
        panel.alpha = 0;
        content.localScale = Vector3.zero;

        // bật interactable
        panel.blocksRaycasts = true;
        panel.interactable = true;

        // hiệu ứng fade + scale (chạy bằng unscaled time)
        panel.DOFade(1f, 0.3f).SetUpdate(true);
        content.DOScale(1f, 0.8f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnComplete(() => content.DOScale(1f, 0.3f).SetUpdate(true));
    }

    public void HidePopup(CanvasGroup panel, RectTransform content)
    {
        // tắt interactable trong lúc ẩn
        panel.blocksRaycasts = false;
        panel.interactable = false;

        // fade out + scale nhỏ lại (chạy bằng unscaled time)
        panel.DOFade(0f, 0.2f).SetUpdate(true);
        content.DOScale(0f, 0.2f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() => panel.gameObject.SetActive(false));
    }

}
