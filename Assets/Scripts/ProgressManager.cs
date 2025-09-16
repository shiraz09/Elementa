using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ProgressManager
/// - שומר stars (0..3) ו-score לכל שלב
/// - highestUnlocked (השלב הגבוה ביותר שנפתח)
/// - RecordResult שומר BEST בלבד (לא מוריד תוצאה טובה קיימת)
/// - תואם לקריאות: RecordResult(stage, score, stars) וגם RecordResult(stage, stars)
/// </summary>
public static class ProgressManager
{
    // ----- Data Model -----
    [Serializable]
    private class ProgressData
    {
        public int highestUnlocked = 1;

        // JsonUtility לא יודע לסדר Dictionary, אז נשמור כרשימות:
        public List<StarEntry>   stars  = new List<StarEntry>();
        public List<ScoreEntry>  scores = new List<ScoreEntry>();

        [NonSerialized] private Dictionary<int,int> _starsMap;
        [NonSerialized] private Dictionary<int,int> _scoresMap;

        [Serializable] public class StarEntry  { public int stage; public int stars; }
        [Serializable] public class ScoreEntry { public int stage; public int score; }

        private void EnsureMaps()
        {
            if (_starsMap == null)
            {
                _starsMap = new Dictionary<int, int>();
                foreach (var e in stars)  _starsMap[e.stage]  = Mathf.Clamp(e.stars, 0, 3);
            }
            if (_scoresMap == null)
            {
                _scoresMap = new Dictionary<int, int>();
                foreach (var e in scores) _scoresMap[e.stage] = Mathf.Max(0, e.score);
            }
        }

        public int GetStars(int stage)
        {
            EnsureMaps();
            return _starsMap.TryGetValue(stage, out var s) ? Mathf.Clamp(s, 0, 3) : 0;
        }

        public int GetScore(int stage)
        {
            EnsureMaps();
            return _scoresMap.TryGetValue(stage, out var sc) ? Mathf.Max(0, sc) : 0;
        }

        public void SetStarsBest(int stage, int starsValue)
        {
            EnsureMaps();
            starsValue = Mathf.Clamp(starsValue, 0, 3);
            int current = GetStars(stage);
            if (starsValue > current) _starsMap[stage] = starsValue;
            SyncStarsList();
        }

        public void SetScoreBest(int stage, int scoreValue)
        {
            EnsureMaps();
            scoreValue = Mathf.Max(0, scoreValue);
            int current = GetScore(stage);
            if (scoreValue > current) _scoresMap[stage] = scoreValue;
            SyncScoresList();
        }

        private void SyncStarsList()
        {
            stars.Clear();
            foreach (var kv in _starsMap)
                stars.Add(new StarEntry { stage = kv.Key, stars = Mathf.Clamp(kv.Value, 0, 3) });
        }

        private void SyncScoresList()
        {
            scores.Clear();
            foreach (var kv in _scoresMap)
                scores.Add(new ScoreEntry { stage = kv.Key, score = Mathf.Max(0, kv.Value) });
        }
    }
    public static int GetStars(int stage)
    {
        return Load().GetStars(stage);
    }

    private struct StageProgress
    {
        public int Stars;
        public int Score;
    }

    // ----- Storage -----
    private const string PlayerPrefsKey = "Progress.v1";
    private static ProgressData _cache;

    private static ProgressData Load()
    {
        if (_cache != null) return _cache;

        var json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
        _cache = string.IsNullOrEmpty(json) ? new ProgressData() : JsonUtility.FromJson<ProgressData>(json);
        if (_cache == null) _cache = new ProgressData();
        if (_cache.highestUnlocked < 1) _cache.highestUnlocked = 1;
        return _cache;
    }

    private static void Save()
    {
        var json = JsonUtility.ToJson(_cache);
        PlayerPrefs.SetString(PlayerPrefsKey, json);
        PlayerPrefs.Save();
    }

    // ----- Public Queries -----
    public static int GetSavedStars(int stage)  => Load().GetStars(stage);
    public static int GetSavedScore(int stage)  => Load().GetScore(stage);
    public static int GetHighestUnlocked()      => Load().highestUnlocked;
    public static bool IsUnlocked(int stage)    => Load().highestUnlocked >= stage;

    // ----- Internal Helpers (נוחות ותאימות לקוד קיים) -----
    private static StageProgress LoadStageProgress(int stage)
    {
        var d = Load();
        return new StageProgress { Stars = d.GetStars(stage), Score = d.GetScore(stage) };
    }
    private static void SetStars(int stage, int stars)   // שומר BEST בלבד
    {
        var d = Load();
        d.SetStarsBest(stage, stars);
        Save();
    }
    private static void SetScore(int stage, int score)   // שומר BEST בלבד
    {
        var d = Load();
        d.SetScoreBest(stage, score);
        Save();
    }

    public static void UnlockUpTo(int stageInclusive)
    {
        var d = Load();
        if (stageInclusive > d.highestUnlocked)
        {
            d.highestUnlocked = stageInclusive;
            Save();
        }
    }
    public static void RecordResult(int stage, int score, int starsEarned)
    {
        var prev = LoadStageProgress(stage);

        int bestStars = Math.Max(prev.Stars, Mathf.Clamp(starsEarned, 0, 3));
        int bestScore = Math.Max(prev.Score, Mathf.Max(0, score));

        SetScore(stage, bestScore);
        SetStars(stage, bestStars);

        if (bestStars >= 1)
            UnlockUpTo(stage + 1);
    }

   
    public static void RecordResult(int stage, int starsEarned)
    {
        RecordResult(stage, 0, starsEarned);
    }
    public static void ResetAll()
    {
        _cache = new ProgressData { highestUnlocked = 1 };
        Save();
    }

    public static void WipeFromDisk()
    {
        PlayerPrefs.DeleteKey(PlayerPrefsKey);
        _cache = null;
    }
}