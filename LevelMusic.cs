using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMusic : MonoBehaviour
{
    AudioManager audioManager;

    private void Start()
    {
        Debug.Log("HELLO FROM " + SceneManager.GetActiveScene().name);
        audioManager = FindAnyObjectByType<AudioManager>();
        audioManager.Play("music" + SceneManager.GetActiveScene().name);
    }
}
