using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button rollButton;
    [SerializeField] private TMP_Text rollButtonText;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject lossPanel;
    [SerializeField] private TMP_Text resultText;

    private void Awake()
    {
        SetPanels(false, false);
    }

    private void Start()
    {
        if (rollButton != null) rollButton.onClick.AddListener(OnRollClicked);
        if (resultText != null) resultText.text = "READY TO ROLL";
    }

    private void OnDestroy()
    {
        if (rollButton != null) rollButton.onClick.RemoveListener(OnRollClicked);
    }

    private void OnRollClicked()
    {
        if (GameManager.Instance != null) GameManager.Instance.RollDice();
    }

    public void SetButtonInteractable(bool value)
    {
        if (rollButton != null) rollButton.interactable = value;
    }

    public void ShowRolling() => Show(false, false, "ROLLING...", "ROLLING...");

    public void ShowWin(string message) => Show(true, false, message, "ROLL AGAIN");

    public void ShowLoss(string message) => Show(false, true, message, "ROLL AGAIN");

    private void Show(bool win, bool loss, string message, string buttonLabel)
    {
        SetPanels(win, loss);
        if (resultText != null) resultText.text = message;
        if (rollButtonText != null) rollButtonText.text = buttonLabel;
    }

    private void SetPanels(bool win, bool loss)
    {
        if (winPanel != null) winPanel.SetActive(win);
        if (lossPanel != null) lossPanel.SetActive(loss);
    }
}