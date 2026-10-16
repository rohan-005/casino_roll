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

    [Header("Pause Menu")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        Time.timeScale = 1f;
        SetPanels(false, false);

        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void Start()
    {
        if (rollButton != null) rollButton.onClick.AddListener(OnRollClicked);
        if (pauseButton != null) pauseButton.onClick.AddListener(PauseGame);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (exitButton != null) exitButton.onClick.AddListener(ExitGame);

        if (resultText != null) resultText.text = "READY TO ROLL";
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;

        if (rollButton != null) rollButton.onClick.RemoveListener(OnRollClicked);
        if (pauseButton != null) pauseButton.onClick.RemoveListener(PauseGame);
        if (resumeButton != null) resumeButton.onClick.RemoveListener(ResumeGame);
        if (exitButton != null) exitButton.onClick.RemoveListener(ExitGame);
    }

    private void OnRollClicked()
    {
        if (GameManager.Instance != null) GameManager.Instance.RollDice();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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