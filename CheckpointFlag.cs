using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointFlag : MonoBehaviour
{
    CheckpointSystem checkpointSystem;
    AudioManager audioManager;
    public ParticleSystem confetti;
    bool checkPointGot = false;

    private void Start()
    {
        checkpointSystem = FindAnyObjectByType<CheckpointSystem>();
        audioManager = FindAnyObjectByType<AudioManager>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (checkpointSystem.spawnPos != (Vector2)transform.position - new Vector2(0f, 1.35f))
        {
            checkPointGot = false;
        }
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!checkPointGot)
            {
                audioManager.Play("Checkpoint");
                confetti.Play();
            }
            checkpointSystem.UpdateCheckpoint(transform.position);
            checkPointGot = true;
        }
    }
}
