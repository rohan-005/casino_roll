using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Dice")]
    [SerializeField] private DiceController[] dice = new DiceController[3];

    [Header("UI")]
    [SerializeField] private GameUI gameUI;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip rollClip;
    [SerializeField] private AudioClip winClip;
    [Range(0f, 1f)]
    [SerializeField] private float winVolume = 1f;

    private const int LossesBeforePity = 3;

    public int ConsecutiveLosses { get; private set; }
    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public void RollDice()
    {
        if (IsPlaying || Time.timeScale == 0f || gameUI == null ||
            dice == null || dice.Length != 3 ||
            Array.IndexOf(dice, null) >= 0)
        {
            return;
        }

        StartCoroutine(RollRoutine());
    }

    private bool AllDiceStopped() =>
        !dice[0].IsRolling && !dice[1].IsRolling && !dice[2].IsRolling;

    private void PlayRollSound()
    {
        if (audioSource == null || rollClip == null) return;

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.clip = rollClip;
        audioSource.loop = false;
        audioSource.Play();
    }

    private void StopRollSound()
    {
        if (audioSource != null) audioSource.Stop();
    }

    private void PlayWinSound()
    {
        if (audioSource == null || winClip == null) return;

        audioSource.pitch = 1f;
        audioSource.PlayOneShot(winClip, winVolume);
    }

    private IEnumerator RollRoutine()
    {
        IsPlaying = true;

        gameUI.SetButtonInteractable(false);
        gameUI.ShowRolling();

        // 0 = normal random roll, 1-6 = guaranteed jackpot on that number.
        int forcedFace = ConsecutiveLosses >= LossesBeforePity ? Random.Range(1, 7) : 0;

        PlayRollSound();

        foreach (DiceController die in dice)
        {
            StartCoroutine(die.Roll(forcedFace));
        }

        yield return new WaitUntil(AllDiceStopped);

        StopRollSound();

        int first = dice[0].DetectTopFace();
        int second = dice[1].DetectTopFace();
        int third = dice[2].DetectTopFace();

        bool jackpot = first != 0 && first == second && second == third;
        string numbers = $"{first} - {second} - {third}";

        if (jackpot)
        {
            ConsecutiveLosses = 0;
            PlayWinSound();
            gameUI.ShowWin($"JACKPOT!\n{numbers}");
        }
        else
        {
            ConsecutiveLosses++;
            gameUI.ShowLoss($"{numbers}");
        }

        IsPlaying = false;
        gameUI.SetButtonInteractable(true);
    }
}