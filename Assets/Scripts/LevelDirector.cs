using UnityEngine;

[DefaultExecutionOrder(-100)]
public class LevelDirector : MonoBehaviour
{
    [Header("Refs")]
    public Grid grid;
    public LevelWithGoals level;

    [Header("Generation")]
    public int stageIndex = 1;
    public bool useFixedSeed = false;
    public int seed = 12345;

    private void Awake()
    {
        if (!grid || !level)
        {
            Debug.LogError("LevelDirector: missing Grid or LevelWithGoals reference.");
            return;
        }

        var spec = useFixedSeed
            ? LevelGenerator.Generate(stageIndex, seed)
            : LevelGenerator.Generate(stageIndex);

        ApplySpec(spec);
    }

    private void ApplySpec(LevelSpec spec)
    {
        // Grid
        grid.baseMoves = spec.maxMoves;
        grid.star1Score = spec.score1Star;
        grid.star2Score = spec.score2Star;
        grid.star3Score = spec.score3Star;
        grid.maxObstacles = spec.maxObstacles;
        grid.obstacleSpawnChance = spec.obstacleSpawnChance;

        // Level
        level.stageIndex = spec.stageIndex;
        level.score1Star = spec.score1Star;
        level.score2Star = spec.score2Star;
        level.score3Star = spec.score3Star;
        level.maxMoves = spec.maxMoves;

        level.goals.Clear();
        foreach (var g in spec.goals)
            level.goals.Add(new CollectGoal { type = g.type, target = g.target, current = 0 });

        Debug.Log($"[Director] Stage {spec.stageIndex} | Moves {spec.maxMoves} | Goals {level.goals.Count} | Obst {spec.maxObstacles} @{spec.obstacleSpawnChance:P0}");
    }
}