using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public TMP_Text movesText;
    public GameObject winPanel;
    public GameObject losePanel;

    public void UpdateMove(int moves)
    {
        movesText.text = "Remaining Moves: " + moves;
    }
    public void UIWin()
    {
        winPanel.SetActive(true);
    }
    public void UILose()
    {
        losePanel.SetActive(true);
    }
}
