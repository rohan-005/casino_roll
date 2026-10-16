using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Dice")]
    [SerializeField] private DiceController[] dice;

    [Header("UI")]
    [SerializeField] private GameUI gameUI;

    private int consecutiveLosses;
    private bool isPlaying;

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
        if (isPlaying)
            return;

        StartCoroutine(RollRoutine());
    }

    private IEnumerator RollRoutine()
    {
        isPlaying = true;

        gameUI.SetButtonInteractable(false);
        gameUI.ShowRolling();

        bool guaranteedWin = consecutiveLosses >= 3;

        // Start all three dice rolls.
        for (int i = 0; i < dice.Length; i++)
        {
            StartCoroutine(dice[i].Roll());
        }

        // Wait for all dice to finish rolling.
        bool rolling = true;

        while (rolling)
        {
            rolling = false;

            foreach (DiceController die in dice)
            {
                if (die.IsRolling)
                {
                    rolling = true;
                    break;
                }
            }

            yield return null;
        }

        // On the fourth attempt, force all dice to the same face.
        if (guaranteedWin)
        {
            int winningNumber = Random.Range(1, 7);

            foreach (DiceController die in dice)
            {
                die.ForceTopFace(winningNumber);
            }

            // Give the physics engine a moment to process the poses.
            yield return new WaitForFixedUpdate();
        }

        int first = dice[0].Result;
        int second = dice[1].Result;
        int third = dice[2].Result;

        bool jackpot =
            first == second &&
            second == third;

        if (jackpot)
        {
            consecutiveLosses = 0;

            gameUI.ShowWin(
                $"JACKPOT! {first} - {second} - {third}"
            );
        }
        else
        {
            consecutiveLosses++;

            gameUI.ShowLoss(
                $"TRY AGAIN {first} - {second} - {third}"
            );
        }

        Debug.Log(
            $"Dice: {first}, {second}, {third} | " +
            $"Consecutive losses: {consecutiveLosses}"
        );

        isPlaying = false;

        gameUI.SetButtonInteractable(true);
    }
}