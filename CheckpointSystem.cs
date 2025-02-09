using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointSystem : MonoBehaviour
{
    Player player;
    public Vector2 spawnPos;
    [SerializeField] Vector2 startPos;

    private void Start()
    {
        player = FindAnyObjectByType<Player>();
        spawnPos = startPos;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SpawnPlayer();
        }
    }

    private void SetStartPos(Vector2 pos)
    {
        startPos = pos;
    }

    public void UpdateCheckpoint(Vector2 pos)
    {
        spawnPos = pos - new Vector2(0f, 1.35f);
    }

    private void SpawnPlayer()
    {
        if (!player.enabled)
        {
            player.enabled = true;
        }
        player.transform.Translate(spawnPos - (Vector2)player.transform.position, Space.World);
    }
}
