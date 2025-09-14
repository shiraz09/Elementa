using UnityEngine;

public class Level : MonoBehaviour
{
    public enum LevelType { TIMER, OBSTACLE, MOVES };

    
    public global::Grid grid;

    public int score1Star;
    public int score2Star;
    public int score3Star;

    protected int currentScore;

    // שדה גיבוי לסוג הלבל (נראה גם באינספקטור)
    [SerializeField] private LevelType levelType;

    // פרופרטי לקריאה בלבד
    protected LevelType Type
    {
        get { return levelType; }
    }

    public virtual void GameWin()
    {
        Debug.Log("You win!");
        grid.GameOver();
    }
    public virtual void GameLose()
    {
        Debug.Log("You Lose");
        grid.GameOver();
    }
    public virtual void OnMove(){}

    public virtual void OnPieceCleared(GamePiece piece)
    {
        currentScore += piece.score;
        Debug.Log("Score: " + currentScore);
    }
    
    public virtual void OnLevelComplete(int finalScore, int starsEarned)
    {
        Debug.Log($"Level completed! Score: {finalScore}, Stars: {starsEarned}");
        
        // כאן תוכל להוסיף לוגיקה נוספת כמו:
        // - שמירת התוצאות
        // - פתיחת שלב הבא
        // - הצגת מסך תוצאות
        // - מתן פרסים
    }
}