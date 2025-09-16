using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelResult
{
    public int stageIndex;
    public int bestScore;
    public int bestStars;
}

[Serializable]
public class ProgressData
{
    public List<LevelResult> results = new List<LevelResult>();
}

public static class ProgressManager
{
    private const string Key = "GAME_PROGRESS_V1";
    private static ProgressData cache;

    public static void RecordResult(int stageIndex, int score, int stars)
    {
        var data = Load();
        var lr = data.results.Find(r => r.stageIndex == stageIndex);
        if (lr == null)
        {
            lr = new LevelResult { stageIndex = stageIndex, bestScore = score, bestStars = stars };
            data.results.Add(lr);
        }
        else
        {
            if (score > lr.bestScore) lr.bestScore = score;
            if (stars > lr.bestStars) lr.bestStars = stars;
        }
        Save(data);
        Debug.Log($"[Progress] Stage {stageIndex} saved: score={lr.bestScore} stars={lr.bestStars}");
    }

    public static LevelResult GetResult(int stageIndex)
    {
        var data = Load();
        return data.results.Find(r => r.stageIndex == stageIndex);
    }

    public static ProgressData Load()
    {
        if (cache != null) return cache;
        var json = PlayerPrefs.GetString(Key, "");
        cache = string.IsNullOrEmpty(json) ? new ProgressData() : JsonUtility.FromJson<ProgressData>(json);
        if (cache == null) cache = new ProgressData();
        return cache;
    }

    public static void Save(ProgressData data)
    {
        cache = data;
        var json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(Key, json);
        PlayerPrefs.Save();
    }

    public static void ResetAll()
    {
        cache = new ProgressData();
        PlayerPrefs.DeleteKey(Key);
    }
}