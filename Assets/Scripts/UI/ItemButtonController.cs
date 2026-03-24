using UnityEngine;
using DG.Tweening;

public class ItemButtonController : MonoBehaviour
{
    [SerializeField] private HintManager hintManager;
    [SerializeField] private ShopController shopController;
    [SerializeField] private HUDController hudController;
    [SerializeField] private BoardManager boardManager;

    [Header("Color Bomb UI")]
    [SerializeField] private RectTransform colorBombButtonTransform;
    [SerializeField] private CanvasGroup colorBombHintGroup;

    private bool isColorBombArmed = false;

    private void Start()
    {
        UpdateColorBombVisual(false);
    }

    public void OnHintButtonClicked()
    {
        if (hintManager == null || ItemManager.Instance == null) return;

        if (!ItemManager.Instance.HasHint())
        {
            if (shopController != null)
            {
                shopController.OpenShop();
                SoundManager.Instance?.PlayClick();
            }
            return;
        }

        bool success = ItemManager.Instance.UseHint();
        if (!success) return;

        hudController?.UpdateItemCounts();
        SoundManager.Instance?.PlayHint();
        hintManager.StartHint();
    }

    public void OnHammerButtonClicked()
    {
        if (ItemManager.Instance == null || boardManager == null) return;

        if (!ItemManager.Instance.HasHammer())
        {
            if (shopController != null)
            {
                shopController.OpenShop();
                SoundManager.Instance?.PlayClick();
            }
            return;
        }

        boardManager.ArmHammer();
    }

    public void OnColorBombButtonClicked()
    {
        if (ItemManager.Instance == null || boardManager == null) return;

        if (!ItemManager.Instance.HasColorBomb())
        {
            if (shopController != null)
            {
                shopController.OpenShop();
                SoundManager.Instance?.PlayClick();
            }
            return;
        }

        if (!isColorBombArmed)
        {
            boardManager.ArmColorBomb();
            isColorBombArmed = true;
            UpdateColorBombVisual(true);
        }
        else
        {
            boardManager.DisarmColorBomb();
            isColorBombArmed = false;
            UpdateColorBombVisual(false);
        }
    }

    public void OnColorBombConsumed()
    {
        isColorBombArmed = false;
        UpdateColorBombVisual(false);
    }

    private void UpdateColorBombVisual(bool armed)
    {
        if (colorBombButtonTransform != null)
        {
            DOTween.Kill(colorBombButtonTransform);
            if (armed)
            {
                colorBombButtonTransform.localScale = Vector3.one;
                colorBombButtonTransform
                    .DOScale(1.15f, 0.35f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
            else
            {
                colorBombButtonTransform.localScale = Vector3.one;
            }
        }

        if (colorBombHintGroup != null)
        {
            if (!colorBombHintGroup.gameObject.activeSelf)
                colorBombHintGroup.gameObject.SetActive(true);

            DOTween.Kill(colorBombHintGroup);
            float targetAlpha = armed ? 1f : 0f;
            colorBombHintGroup.alpha = targetAlpha;

            colorBombHintGroup.interactable = false;
            colorBombHintGroup.blocksRaycasts = false;
        }
    }

}