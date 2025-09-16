using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelSpec
{
    public int stageIndex;

    public int maxMoves;

    public int score1Star;
    public int score2Star;
    public int score3Star;

    public int maxObstacles;
    [Range(0f, 1f)] public float obstacleSpawnChance;

    public List<CollectGoal> goals = new List<CollectGoal>();
}