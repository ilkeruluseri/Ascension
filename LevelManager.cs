using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        Time.timeScale = 1f;  // Just to make sure;
    }

    public void LoadLevel(int level)
    {
        Time.timeScale = 1f; // Just to make sure;
        string levelToLoad = "Level" + level;
        SceneManager.LoadScene(levelToLoad);
    }

    public void LoadScreen(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
