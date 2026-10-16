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
    }

    public void RollDice()
    {
        if (IsPlaying || gameUI == null ||
            dice == null || dice.Length != 3 ||
            Array.IndexOf(dice, null) >= 0)
        {
            return;
        }

        StartCoroutine(RollRoutine());
    }

    private bool AllDiceStopped() =>
        !dice[0].IsRolling && !dice[1].IsRolling && !dice[2].IsRolling;

    private IEnumerator RollRoutine()
    {
        IsPlaying = true;

        gameUI.SetButtonInteractable(false);
        gameUI.ShowRolling();

        // 0 = normal random roll, 1-6 = guaranteed jackpot on that number.
        int forcedFace = ConsecutiveLosses >= LossesBeforePity ? Random.Range(1, 7) : 0;

        foreach (DiceController die in dice)
        {
            StartCoroutine(die.Roll(forcedFace));
        }

        yield return new WaitUntil(AllDiceStopped);

        int first = dice[0].DetectTopFace();
        int second = dice[1].DetectTopFace();
        int third = dice[2].DetectTopFace();

        bool jackpot = first != 0 && first == second && second == third;
        string numbers = $"{first} - {second} - {third}";

        if (jackpot)
        {
            ConsecutiveLosses = 0;
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