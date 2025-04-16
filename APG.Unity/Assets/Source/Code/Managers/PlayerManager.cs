using System;
using System.Collections;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    private int lifes;
    [SerializeField]
    private float respawnTime = 3.0f;

    private PlayerUI _ui;
    private Vector3 _playerSpawn;
    private Player _player;

    private Coroutine _respawnCoroutine;

    private void Awake()
    {
        _ui = FindFirstObjectByType<PlayerUI>();
        _player = FindFirstObjectByType<Player>();
        _playerSpawn = _player.transform.position;
    }

    private void Start()
    {
        _ui.UpdateLifes(lifes);
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        _player.DisableControls();
        _player.gameObject.SetActive(false);
        _player.transform.position = _playerSpawn;
        _player.gameObject.SetActive(true);
        _player.Spawn();
        _player.EnableControls();
    }

    public void AddLife()
    {
        lifes++;
    }

    public void KillPlayer()
    {
        _player.DisableControls();
        _player.Kill();

        lifes--;
        _ui.UpdateLifes(lifes);

        if (lifes > 0)
        {
            if(_respawnCoroutine != null)
                StopCoroutine(_respawnCoroutine);
            StartCoroutine(Respawn());
        }
        else
        {
            //Game Over
            FindFirstObjectByType<GameManager>().LoseGame();
        }
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        SpawnPlayer();
    }
}
