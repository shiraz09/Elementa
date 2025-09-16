using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelLoader
{
    public static int SelectedStage { get; private set; } = 1;

    public static void LoadStage(int stageIndex, string gameplaySceneName = "Main")
    {
        SelectedStage = Mathf.Max(1, stageIndex);
        SceneManager.LoadScene(gameplaySceneName);
    }
}