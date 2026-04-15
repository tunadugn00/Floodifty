using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public enum TutorialStep
{
    Start,
    Hint,
    Fill,
    Hammer,
    Bomb,
    Done
}

public class TutorialManager : MonoBehaviour
{
    [Header("Managers")]
    public BoardManager boardManager;
    public HintManager hintManager;
    public GameObject tutorialLayer;

    [Header("Item Buttons")]
    public Button hintButton;
    public Button hammerButton;
    public Button colorBombButton;

    [Header("Tutorial UI")]
    public CanvasGroup darkOverlay;
    public TextMeshProUGUI instructionText;
    public RectTransform instructionBox;
    public RectTransform highlightFrame;

    [Header("Settings")]
    [SerializeField] private float highlightPulseScale = 1.15f;
    [SerializeField] private float pulseDuration = 0.5f;
    private const string PREF_KEY = "TutorialDone_v1";

    private TutorialStep currentStep = TutorialStep.Start;
    private bool stepDone = false;

    void Start()
    {
        bool shouldRun = !GameManager.Instance.isEndlessMode
                      && PlayerPrefs.GetInt("SelectedLevel", 1) == 1
                      && PlayerPrefs.GetInt(PREF_KEY, 0) == 0;

        if (!shouldRun)
        {
            if (tutorialLayer != null) tutorialLayer.SetActive(false);
            return;
        }
        ItemManager.Instance.StartTutorialMode();
        boardManager.hudController?.UpdateItemCounts();

        HideAllUI();
        StartCoroutine(RunTutorial());
    }

    private IEnumerator RunTutorial()
    {
        yield return new WaitForSeconds(1.5f);
        boardManager.SetUIBlocking(true);

        while (currentStep != TutorialStep.Done)
        {
            stepDone = false;
            yield return StartCoroutine(RunStep(currentStep));
            yield return new WaitUntil(() => stepDone);
        }

        HideAllUI();
        boardManager.SetUIBlocking(false);

        ItemManager.Instance.EndTutorialMode();
        boardManager.hudController?.UpdateItemCounts();

        PlayerPrefs.SetInt(PREF_KEY, 1);
        PlayerPrefs.Save();
        if (tutorialLayer != null) tutorialLayer.SetActive(false);
    }

    private IEnumerator RunStep(TutorialStep step)
    {
        switch (step)
        {
            case TutorialStep.Start: yield return StepStart(); break;
            case TutorialStep.Hint: yield return StepHint(); break;
            case TutorialStep.Fill: yield return StepFill(); break;
            case TutorialStep.Hammer: yield return StepHammer(); break;
            case TutorialStep.Bomb: yield return StepBomb(); break;
        }
    }

    private void NextStep()
    {
        currentStep++;
        stepDone = true;
    }

    private IEnumerator StepStart()
    {
        ShowOverlay();
        ShowInstruction("Tô hết bảng theo màu mục tiêu!\r\nChạm để tiếp tục.");
        HideHighlight();

        yield return WaitForScreenTap();
        NextStep();
    }

    private IEnumerator StepHint()
    {
        ShowInstruction("Dùng Gợi ý để xem nước đi tốt nhất!");
        ShowHighlight(hintButton?.GetComponent<RectTransform>(), new Vector2(200, 150));

        boardManager.SetUIBlocking(false);
        yield return WaitForButtonTap(hintButton);
        boardManager.SetUIBlocking(true);

        yield return new WaitForSeconds(2.5f);
        HideHighlight();
        NextStep();
    }

    private IEnumerator StepFill()
    {
        Tile.TileColor fillColor = GetSuggestedColor();
        RectTransform colorBtnRect = GetColorButtonRect(fillColor);

        ShowInstruction("Chọn màu này để tô!");
        ShowHighlight(colorBtnRect, new Vector2(300, 200));

        boardManager.SetUIBlocking(false);
        yield return WaitForColorButtonTap(fillColor);

        ShowInstruction("Chạm bảng để tô!");
        HideHighlight();

        bool tileTapped = false;
        void OnTileTapped() => tileTapped = true;
        boardManager.OnTileClickedEvent += OnTileTapped;

        yield return new WaitUntil(() => tileTapped);
        boardManager.OnTileClickedEvent -= OnTileTapped;
        boardManager.SetUIBlocking(true);

        yield return new WaitForSeconds(1.2f);
        NextStep();
    }

    private IEnumerator StepHammer()
    {
        ShowInstruction("Dùng Búa để phá đá!");
        ShowHighlight(hammerButton?.GetComponent<RectTransform>(), new Vector2(200, 150));

        bool hammerArmed = false;
        void OnHammerArmed() => hammerArmed = true;
        boardManager.OnHammerArmed += OnHammerArmed;

        boardManager.SetUIBlocking(false);

        yield return new WaitUntil(() => hammerArmed);
        boardManager.OnHammerArmed -= OnHammerArmed;

        ShowInstruction("Chạm vào đá để phá!");

        Tile[,] tiles = boardManager.GetTiles();
        int rockR = -1, rockC = -1;
        for (int r = 0; r < tiles.GetLength(0); r++)
        {
            for (int c = 0; c < tiles.GetLength(1); c++)
            {
                if (tiles[r, c].isRock)
                {
                    rockR = r; rockC = c; break;
                }
            }
        }
        if (rockR != -1) ShowHighlightOnTile(rockR, rockC, new Vector2(200, 150));

        bool hammerDone = false;
        void OnHammerUsed() => hammerDone = true;
        boardManager.OnHammerUsed += OnHammerUsed;

        yield return new WaitUntil(() => hammerDone);
        boardManager.OnHammerUsed -= OnHammerUsed;
        boardManager.SetUIBlocking(true);

        yield return new WaitForSeconds(1f);
        NextStep();
    }

    private IEnumerator StepBomb()
    {
        ShowInstruction("Bom màu đổi toàn bộ một màu!");
        ShowHighlight(colorBombButton?.GetComponent<RectTransform>(), new Vector2(200, 150));

        bool bombArmed = false;
        void OnBombArmed() => bombArmed = true;
        boardManager.OnColorBombArmed += OnBombArmed;

        boardManager.SetUIBlocking(false);
        yield return new WaitUntil(() => bombArmed);
        boardManager.OnColorBombArmed -= OnBombArmed;

        Tile.TileColor bombColor = GetNonGoalColor();
        RectTransform colorBtnRect = GetColorButtonRect(bombColor);

        ShowInstruction("Chọn màu trước!");
        ShowHighlight(colorBtnRect, new Vector2(300, 200));

        yield return WaitForColorButtonTap(bombColor);

        ShowInstruction("Bây giờ chạm vào một ô trên bảng để kích hoạt!");

        Tile[,] tiles = boardManager.GetTiles();
        Tile.TileColor goalColor = boardManager.GetGoalColor();
        int targetR = 0, targetC = 0;

        for (int r = 0; r < tiles.GetLength(0); r++)
        {
            for (int c = 0; c < tiles.GetLength(1); c++)
            {
                if (!tiles[r, c].isRock && tiles[r, c].Color != bombColor && tiles[r, c].Color != goalColor)
                {
                    targetR = r; targetC = c;
                    break;
                }
            }
        }
        ShowHighlightOnTile(targetR, targetC, new Vector2(200, 150));

        bool bombDone = false;
        void OnBombUsed() => bombDone = true;
        boardManager.OnColorBombUsed += OnBombUsed;

        yield return new WaitUntil(() => bombDone);
        boardManager.OnColorBombUsed -= OnBombUsed;
        boardManager.SetUIBlocking(true);

        yield return new WaitForSeconds(1.5f);
        NextStep();
    }

    private Tile.TileColor GetNonGoalColor()
    {
        Tile.TileColor goal = boardManager.GetGoalColor();
        foreach (var btn in boardManager.colorButtons)
        {
            if (btn != null && (Tile.TileColor)btn.colorType != goal)
                return (Tile.TileColor)btn.colorType;
        }
        return goal;
    }

    private void ShowOverlay()
    {
        if (darkOverlay == null) return;
        darkOverlay.gameObject.SetActive(true);
        darkOverlay.alpha = 0f;
        darkOverlay.DOFade(0.6f, 0.3f);
    }

    private void HideOverlay()
    {
        if (darkOverlay == null) return;
        darkOverlay.DOFade(0f, 0.3f)
            .OnComplete(() => darkOverlay.gameObject.SetActive(false));
    }

    private void ShowInstruction(string text)
    {
        if (instructionText != null) instructionText.text = text;
        if (instructionBox != null)
        {
            instructionBox.gameObject.SetActive(true);
            instructionBox.DOKill();
            instructionBox.localScale = Vector3.zero;
            instructionBox.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
        }
    }

    private void ShowHighlight(RectTransform target, Vector2? customSize = null)
    {
        if (highlightFrame == null || target == null) return;

        highlightFrame.gameObject.SetActive(true);
        highlightFrame.position = target.position;

        if (customSize.HasValue)
        {
            highlightFrame.sizeDelta = customSize.Value;
        }
        else
        {
            highlightFrame.sizeDelta = target.sizeDelta;
        }

        highlightFrame.DOKill();
        highlightFrame.localScale = Vector3.one;
        highlightFrame.DOScale(highlightPulseScale, pulseDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void ShowHighlightOnTile(int row, int col, Vector2 customSize)
    {
        if (highlightFrame == null) return;

        Tile[,] tiles = boardManager.GetTiles();
        if (row < 0 || row >= tiles.GetLength(0) || col < 0 || col >= tiles.GetLength(1)) return;

        Tile targetTile = tiles[row, col];
        if (targetTile == null) return;

        highlightFrame.gameObject.SetActive(true);

        Vector2 screenPos = Camera.main.WorldToScreenPoint(targetTile.transform.position);
        highlightFrame.position = screenPos;
        highlightFrame.sizeDelta = customSize;

        highlightFrame.DOKill();
        highlightFrame.localScale = Vector3.one;
        highlightFrame.DOScale(highlightPulseScale, pulseDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
    private void HideHighlight()
    {
        if (highlightFrame == null) return;
        highlightFrame.DOKill();
        highlightFrame.gameObject.SetActive(false);

    }

    private void HideAllUI()
    {
        if (darkOverlay != null) darkOverlay.gameObject.SetActive(false);
        if (instructionBox != null) instructionBox.gameObject.SetActive(false);
        HideHighlight();
    }

    private IEnumerator WaitForScreenTap()
    {
        yield return new WaitForSeconds(0.4f);
        while (!Input.GetMouseButtonDown(0) && !HasTouch())
            yield return null;
        yield return new WaitForSeconds(0.1f);
    }

    private IEnumerator WaitForButtonTap(Button button)
    {
        if (button == null) yield break;
        bool tapped = false;
        void Handler() => tapped = true;
        button.onClick.AddListener(Handler);
        yield return new WaitUntil(() => tapped);
        button.onClick.RemoveListener(Handler);
    }

    private IEnumerator WaitForColorButtonTap(Tile.TileColor targetColor)
    {
        bool selected = false;
        void Handler() => selected = true;

        foreach (var btn in boardManager.colorButtons)
        {
            if (btn != null && (Tile.TileColor)btn.colorType == targetColor)
            {
                var b = btn.GetComponent<Button>();
                if (b != null) b.onClick.AddListener(Handler);
            }
        }

        yield return new WaitUntil(() => selected);

        foreach (var btn in boardManager.colorButtons)
        {
            if (btn != null && (Tile.TileColor)btn.colorType == targetColor)
            {
                var b = btn.GetComponent<Button>();
                if (b != null) b.onClick.RemoveListener(Handler);
            }
        }
    }

    private IEnumerator WaitForTileClicked()
    {
        yield return new WaitForSeconds(0.3f);
        while (!Input.GetMouseButtonDown(0) && !HasTouch())
            yield return null;
        yield return new WaitForSeconds(0.1f);
    }

    private bool HasTouch()
    {
        return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
    }

    private Tile.TileColor GetSuggestedColor()
    {
        foreach (var btn in boardManager.colorButtons)
        {
            if (btn != null && btn.transform.localScale.x > 1.05f)
                return (Tile.TileColor)btn.colorType;
        }
        return boardManager.GetGoalColor();
    }

    private RectTransform GetColorButtonRect(Tile.TileColor color)
    {
        foreach (var btn in boardManager.colorButtons)
        {
            if (btn != null && (Tile.TileColor)btn.colorType == color)
                return btn.GetComponent<RectTransform>();
        }
        return null;
    }
}