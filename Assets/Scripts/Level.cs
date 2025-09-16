using UnityEngine;

public class Level : MonoBehaviour
{
    public enum LevelType { TIMER, OBSTACLE, MOVES }

    [Header("Refs")]
    public Grid grid;

    [Header("Stars thresholds")]
    public int score1Star = 600;
    public int score2Star = 950;
    public int score3Star = 1400;

    [Header("Meta")]
    public int stageIndex = 1;

    protected int currentScore;

    // קריאה כשניצחנו/הפסדנו
    public virtual void GameWin()
    {
        Debug.Log("You win!");
        grid?.GameOver();
    }

    public virtual void GameLose()
    {
        Debug.Log("You lose!");
        grid?.GameOver();
    }

    // קריאה מה-Grid אחרי החלפה חוקית
    public virtual void OnMove() {}

    // קריאה מה-Grid על כל מחיקה של חתיכה רגילה
    public virtual void OnPieceCleared(GamePiece piece)
    {
        if (piece != null) currentScore += piece.score;
        // עדכון ניקוד UI יכול להיעשות דרך grid.gameUI.UpdateUI()
    }

    // לא חובה להשתמש, אבל נשאיר אם תרצי מסך סיכום
    public virtual void OnLevelComplete(int finalScore, int starsEarned)
    {
        Debug.Log($"Level completed! Score: {finalScore}, Stars: {starsEarned}");
    }
}