using UnityEngine;

public class ItemButtonController : MonoBehaviour
{
    [SerializeField] private HintManager hintManager;
    [SerializeField] private ShopController shopController;
    [SerializeField] private HUDController hudController;
    [SerializeField] private BoardManager boardManager;

    // 1. Nút Hint – dùng flow cũ, nhưng gói lại
    public void OnHintButtonClicked()
    {
        if (hintManager == null || ItemManager.Instance == null) return;

        // Nếu hết Hint -> mở shop
        if (!ItemManager.Instance.HasHint())
        {
            if (shopController != null)
            {
                shopController.OpenShop();
                SoundManager.Instance?.PlayClick();
            }
            return;
        }

        // Còn Hint -> dùng 1 cái và cho HintManager chạy logic MCTS
        bool success = ItemManager.Instance.UseHint();
        if (!success) return;

        hudController?.UpdateItemCounts();
        hintManager.StartHint();
    }

    // 2. Nút Hammer – tạm thời mới xử lý “mua hoặc mở shop”
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

    // 3. Nút Color Bomb – tương tự Hammer, để TODO
    public void OnColorBombButtonClicked()
    {
        if (ItemManager.Instance == null) return;

        if (!ItemManager.Instance.HasColorBomb())
        {
            if (shopController != null)
            {
                shopController.OpenShop();
                SoundManager.Instance?.PlayClick();
            }
            return;
        }

        // TODO: bước sau implement Color Bomb
        SoundManager.VibrateIfEnabled();
    }
}