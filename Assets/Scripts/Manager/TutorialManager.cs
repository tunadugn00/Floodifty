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
    public GameObject tapToContinueObj;
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
        ShowInstruction("Fill toàn bộ ô thành màu mục tiêu!\nTap để tiếp tục.");
        ShowTapToContinue(true);
        HideHighlight();

        yield return WaitForScreenTap();
        ShowTapToContinue(false);
        NextStep();
    }

    private IEnumerator StepHint()
    {
        ShowInstruction("Dùng Hint để xem gợi ý bước đi tốt nhất!");
        ShowHighlight(hintButton?.GetComponent<RectTransform>());
        ShowTapToContinue(false);

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

        ShowInstruction("Chọn màu này để fill!");
        ShowHighlight(colorBtnRect);

        boardManager.SetUIBlocking(false);
        yield return WaitForColorButtonTap(fillColor);

        ShowInstruction("Tap vào board để fill!");
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
        ShowInstruction("Dùng Hammer để phá ô đá!");
        ShowHighlight(hammerButton?.GetComponent<RectTransform>());

        bool hammerArmed = false;
        void OnHammerArmed() => hammerArmed = true;
        boardManager.OnHammerArmed += OnHammerArmed;

        boardManager.SetUIBlocking(false);

        yield return new WaitUntil(() => hammerArmed);
        boardManager.OnHammerArmed -= OnHammerArmed;

        ShowInstruction("Tap vào ô đá để phá!");
        HideHighlight();

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
        ShowInstruction("Color Bomb đổi toàn bộ một màu thành màu đang chọn!");
        ShowHighlight(colorBombButton?.GetComponent<RectTransform>());

        bool bombArmed = false;
        void OnBombArmed() => bombArmed = true;
        boardManager.OnColorBombArmed += OnBombArmed;

        boardManager.SetUIBlocking(false);

        yield return new WaitUntil(() => bombArmed);
        boardManager.OnColorBombArmed -= OnBombArmed;

        ShowInstruction("Tap vào board để kích hoạt Bomb!");
        HideHighlight();

        bool bombDone = false;
        void OnBombUsed() => bombDone = true;
        boardManager.OnColorBombUsed += OnBombUsed;

        yield return new WaitUntil(() => bombDone);
        boardManager.OnColorBombUsed -= OnBombUsed;
        boardManager.SetUIBlocking(true);

        yield return new WaitForSeconds(1.5f);
        NextStep();
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

    private void ShowHighlight(RectTransform target)
    {
        if (highlightFrame == null || target == null) return;

        highlightFrame.gameObject.SetActive(true);
        highlightFrame.position = target.position;
        highlightFrame.sizeDelta = target.sizeDelta * 1.15f;

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

    private void ShowTapToContinue(bool show)
    {
        tapToContinueObj?.SetActive(show);
    }

    private void HideAllUI()
    {
        if (darkOverlay != null) darkOverlay.gameObject.SetActive(false);
        if (instructionBox != null) instructionBox.gameObject.SetActive(false);
        HideHighlight();
        ShowTapToContinue(false);
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