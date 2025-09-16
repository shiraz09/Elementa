using System.Collections.Generic;
using UnityEngine;

public static class LevelGenerator
{
    private static readonly Grid.PieceType[] Regular =
    {
        Grid.PieceType.EARTH, Grid.PieceType.GRASS,
        Grid.PieceType.WATER, Grid.PieceType.SUN
    };

    public static LevelSpec Generate(int stageIndex, int? seed = null)
    {
        var rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        int tier = Mathf.Clamp((stageIndex - 1) / 5, 0, 10);

        if (stageIndex == 1)
        {
            return new LevelSpec
            {
                stageIndex = 1,
                maxMoves = 24,
                score1Star = 600, score2Star = 950, score3Star = 1400,
                maxObstacles = 0, obstacleSpawnChance = 0f,
                goals = new List<CollectGoal>()
            };
        }

        int maxMoves = Mathf.Max(12, 24 - Mathf.Min(stageIndex / 6, 6));
        int maxObs = (stageIndex <= 2) ? 0 : Mathf.Clamp(2 + tier * 2, 2, 14);
        float obsChance = (stageIndex <= 2) ? 0f : Mathf.Clamp01(0.08f + tier * 0.03f);

        int minGoals = (stageIndex <= 2) ? 1 : (tier >= 2 ? 2 : 1);
        int maxGoals = (tier >= 3) ? 3 : 2;
        int goalsCount = rng.Next(minGoals, maxGoals + 1);

        (int lo, int hi) GoalRange(int s)
        {
            int a = Mathf.Clamp(6 + s, 8, 25);
            int b = Mathf.Clamp(10 + s * 2, 14, 40);
            if (b < a) b = a + 2;
            return (a, b);
        }
        var (gLo, gHi) = GoalRange(stageIndex);

        int s1 = 600 + stageIndex * 60;
        int s2 = 950 + stageIndex * 85;
        int s3 = 1400 + stageIndex * 110;

        var pool = new List<Grid.PieceType>(Regular);
        Shuffle(pool, rng);
        goalsCount = Mathf.Clamp(goalsCount, 1, Mathf.Min(maxGoals, pool.Count));

        var goals = new List<CollectGoal>();
        for (int i = 0; i < goalsCount; i++)
        {
            int target = rng.Next(gLo, gHi + 1);
            goals.Add(new CollectGoal { type = pool[i], target = target, current = 0 });
        }

        if (goals.Count >= 3) maxMoves += 2;
        if (maxObs == 0 && goals.Count > 0) maxMoves -= 1;
        maxMoves = Mathf.Max(10, maxMoves);

        return new LevelSpec
        {
            stageIndex = stageIndex,
            maxMoves = maxMoves,
            score1Star = s1, score2Star = s2, score3Star = s3,
            maxObstacles = maxObs, obstacleSpawnChance = obsChance,
            goals = goals
        };
    }

    private static void Shuffle<T>(IList<T> list, System.Random rng)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = rng.Next(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}