using UnityEngine;
using System.Collections.Generic;

public class LevelWithGoals : Level
{
    [Header("Moves (קובע גם ב-Grid)")]
    public int maxMoves = 20;

    [Header("Collect Goals")]
    public List<CollectGoal> goals = new List<CollectGoal>();

    private void Start()
    {
        if (grid != null)
        {
            grid.baseMoves = maxMoves;
            grid.CalculateMovesForLevel();

            grid.gameUI?.ShowGoals(goals);
            grid.gameUI?.UpdateGoals(goals);
            grid.gameUI?.UpdateUI();
        }
    }

    public override void OnMove()
    {
        TryCheckWin();
    }

    public override void OnPieceCleared(GamePiece piece)
    {
        base.OnPieceCleared(piece);

        if (piece != null &&
            piece.Type != Grid.PieceType.ICEOBS &&
            piece.Type != Grid.PieceType.GRASSOBS)
        {
            for (int i = 0; i < goals.Count; i++)
                if (goals[i].type == piece.Type) { goals[i].current++; break; }

            grid?.gameUI?.UpdateGoals(goals);
            grid?.gameUI?.UpdateUI();
        }

        TryCheckWin();
    }

    // נקרא מה-Grid כשנגמרים המהלכים
    public void OnMovesDepleted()
    {
        bool haveStar = currentScore >= score1Star;
        bool goalsDone = AllGoalsDone();

        if (goalsDone && haveStar) GameWin();
        else GameLose();
    }

    private bool AllGoalsDone()
    {
        for (int i = 0; i < goals.Count; i++)
            if (goals[i].current < goals[i].target) return false;
        return true;
    }

    private void TryCheckWin()
    {
        // אם יש 3 כוכבים – אין טעם להמשיך
        if (GetStarCount() == 3)
        {
            GameWin();
            return;
        }

        if (AllGoalsDone() && currentScore >= score1Star)
            GameWin();
    }

    public override void GameWin()
    {
        base.GameWin();
        ProgressManager.RecordResult(stageIndex, currentScore, GetStarCount());
    }

    public override void GameLose()
    {
        base.GameLose();
        ProgressManager.RecordResult(stageIndex, currentScore, GetStarCount());
    }

    private int GetStarCount()
    {
        if (currentScore >= score3Star) return 3;
        if (currentScore >= score2Star) return 2;
        if (currentScore >= score1Star) return 1;
        return 0;
    }
}