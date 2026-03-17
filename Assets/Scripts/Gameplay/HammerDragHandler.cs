using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HammerDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform hammerIconPrefab; 
    [SerializeField] private Canvas uiCanvas;               
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private float dragThreshold = 12f;

    private RectTransform activeHammerIcon;
    private Vector2 pointerDownPos;
    private bool isDragging;

    private RectTransform CanvasRect => uiCanvas != null ? uiCanvas.transform as RectTransform : null;

    private void UpdateIconPosition(PointerEventData eventData)
    {
        if (activeHammerIcon == null) return;
        var canvasRect = CanvasRect;
        if (canvasRect == null) return;

        Camera cam = eventData.pressEventCamera;
        if (uiCanvas != null && uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            cam = null;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, cam, out var localPoint))
        {
            activeHammerIcon.anchoredPosition = localPoint;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (ItemManager.Instance == null || !ItemManager.Instance.HasHammer()) 
        {
            SoundManager.VibrateIfEnabled();
            return;
        }

        if (hammerIconPrefab == null || uiCanvas == null) return;

        pointerDownPos = eventData.position;
        isDragging = false;

        activeHammerIcon = Instantiate(hammerIconPrefab);
        activeHammerIcon.SetParent(uiCanvas.transform, false);
        activeHammerIcon.SetAsLastSibling();
        UpdateIconPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (activeHammerIcon == null) return;

        if (!isDragging && Vector2.Distance(eventData.position, pointerDownPos) >= dragThreshold)
            isDragging = true;

        UpdateIconPosition(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (activeHammerIcon != null)
            Destroy(activeHammerIcon.gameObject);

        if (boardManager == null || Camera.main == null) return;

        // Nếu chỉ tap (không kéo), thì "arm" búa để người chơi tap tiếp vào đá.
        if (!isDragging)
        {
            boardManager.ArmHammer();
            return;
        }

        // Raycast từ màn hình xuống world để tìm Tile (mobile: eventData.position là vị trí touch)
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(worldPos2D, Vector2.zero);
        if (hit.collider == null) return;

        Tile tile = hit.collider.GetComponent<Tile>();
        if (tile == null) return;

        // Dùng Hammer lên ô đó
        boardManager.UseHammerOnTile(tile.Row, tile.Col);
    }
}