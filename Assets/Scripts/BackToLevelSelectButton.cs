using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToLevelSelectButton : MonoBehaviour
{
    [SerializeField] private string levelSelectScene = "LevelSelect";

    // מחובר ל-OnClick של הכפתור
    public void Go()
    {
        // לוודא שאין פאוז פעיל
        Time.timeScale = 1f;
        AudioListener.pause = false;

        SceneManager.LoadScene(levelSelectScene);
    }
}