
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButtonHandler : MonoBehaviour
{
    public void LoadMainScene()
    {
        SceneManager.LoadScene("LevelSelect");   
    }
}
