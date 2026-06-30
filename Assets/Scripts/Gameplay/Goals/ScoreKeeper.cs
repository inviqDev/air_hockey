using UnityEngine;

public struct GoalResult
{
    public GoalResult(PlayerSide scoringSide, int leftScore, int rightScore, bool hasWinner)
    {
        ScoringSide = scoringSide;
        LeftScore = leftScore;
        RightScore = rightScore;
        HasWinner = hasWinner;
    }

    public PlayerSide ScoringSide { get; }
    public int LeftScore { get; }
    public int RightScore { get; }
    public bool HasWinner { get; }
}

public sealed class ScoreKeeper : MonoBehaviour
{
    [SerializeField] private int winningScore = 7;

    public int LeftScore { get; private set; }
    public int RightScore { get; private set; }

    public GoalResult RegisterGoal(PlayerSide goalSide)
    {
        var scoringSide = goalSide == PlayerSide.Left ? PlayerSide.Right : PlayerSide.Left;

        if (scoringSide == PlayerSide.Left)
            LeftScore++;
        else
        {
            RightScore++;
        }

        return new GoalResult(scoringSide, LeftScore, RightScore, HasWinner());
    }

    public void ResetScores()
    {
        LeftScore = 0;
        RightScore = 0;
    }

    private bool HasWinner()
    {
        return winningScore > 0 && (LeftScore >= winningScore || RightScore >= winningScore);
    }
}
