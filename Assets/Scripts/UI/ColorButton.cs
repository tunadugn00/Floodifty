using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ColorButton : MonoBehaviour
{
    public Tile.TileColor colorType;
    public Image iconImage;
    public RectTransform rectTransform;

    [Header("Selected Glow")]
    [Tooltip("LightingEffect01 (or its GameObject) placed behind the button icon.")]
    [SerializeField] private GameObject lightingEffect01;

    [SerializeField] private float glowScaleSelected = 1.05f;
    void Awake()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (lightingEffect01 != null)
            lightingEffect01.SetActive(false);
    }
    public void OnClick()
    {
        FindFirstObjectByType<BoardManager>().OnColorSelected(colorType);
    }
    public void UpdateVisualState(Tile.TileColor selectedColor, float scaleTime = 0.15f)
    {
        bool isSelected = (colorType == selectedColor);

        if (isSelected)
        {
            rectTransform.DOScale(1.15f, 0.15f).SetEase(Ease.OutBack).SetUpdate(true);
            if (iconImage != null)
            {
                iconImage.DOColor(Color.white, 0.15f).SetUpdate(true);
            }

            if (lightingEffect01 != null)
            {
                lightingEffect01.SetActive(true);
                lightingEffect01.transform.localScale = Vector3.one;
                lightingEffect01.transform
                    .DOScale(glowScaleSelected, scaleTime)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);
            }
        }
        else
        {
            rectTransform.DOScale(1.0f, 0.15f).SetUpdate(true);
            if (iconImage != null)
            {
                iconImage.DOColor(new Color(0.8f, 0.8f, 0.8f, 1f), 0.15f).SetUpdate(true);
            }

            if (lightingEffect01 != null)
            {
                lightingEffect01.SetActive(false);
            }
        }
    }
}
