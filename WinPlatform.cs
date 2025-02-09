using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinPlatform : MonoBehaviour
{
    [SerializeField] GameObject winPopUp;
    [SerializeField] ParticleSystem confetti;
    AudioManager audioManager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            audioManager = FindAnyObjectByType<AudioManager>();
            audioManager.Play("Win");
            confetti.Play();
            Time.timeScale = 0f;
            winPopUp.SetActive(true);
            Debug.Log("VICTORY!");
        }
    }
}
