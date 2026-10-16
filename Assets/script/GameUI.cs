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
        if (winPanel != null)
            winPanel.SetActive(false);

        if (lossPanel != null)
            lossPanel.SetActive(false);
    }

    private void Start()
    {
        rollButton.onClick.AddListener(OnRollClicked);
    }

    private void OnDestroy()
    {
        if (rollButton != null)
            rollButton.onClick.RemoveListener(OnRollClicked);
    }

    private void OnRollClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RollDice();
        }
    }

    public void SetButtonInteractable(bool value)
    {
        rollButton.interactable = value;
    }

    public void ShowRolling()
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        if (lossPanel != null)
            lossPanel.SetActive(false);

        if (rollButtonText != null)
            rollButtonText.text = "ROLLING...";
    }

    public void ShowWin(string message)
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        if (lossPanel != null)
            lossPanel.SetActive(false);

        if (resultText != null)
            resultText.text = message;

        if (rollButtonText != null)
            rollButtonText.text = "ROLL AGAIN";
    }

    public void ShowLoss(string message)
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        if (lossPanel != null)
            lossPanel.SetActive(true);

        if (resultText != null)
            resultText.text = message;

        if (rollButtonText != null)
            rollButtonText.text = "ROLL AGAIN";
    }
}