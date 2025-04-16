using System;
using System.Collections.Generic;
using System.Linq;
using APG.Unity.Commands;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    private List<Enemy> _enemies = new List<Enemy>();

    private void Awake()
    {
        _enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
    }

    private void Start()
    {
        foreach (var enemy in _enemies)
        {
            enemy.RandomizeDirection();
            enemy.RandomizeMovementSpeed();
        }
    }

    public void KillEnemiesByAmmount(int ammount)
    {
        if (ammount >= _enemies.Count)
        {
            KillAllEnemies();
            return;
        }

        for (int i = 0; i < ammount; i++)
        {
            KillRandomEnemy();
        }
    }

    public void KillAllEnemies()
    {
        while (_enemies.Count > 0)
        {
            KillRandomEnemy();
        }
    }

    public void KillRandomEnemy()
    {
        var enemy = _enemies[Random.Range(0, _enemies.Count)];
        _enemies.Remove(enemy);
        enemy.Kill();
    }

    //APG Complex Parameters

    public void KillByName(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            var enemy = _enemies.FirstOrDefault(e => e.gameObject.name == name);
            if (enemy == null)
                return;
            _enemies.Remove(enemy);
            enemy.Kill();
        }
    }

    public void KillByID(int id)
    {
        if (id != 0)
        {
            if (id - 1 < _enemies.Count)
            {
                var enemy = _enemies[id - 1];
                _enemies.Remove(enemy);
                enemy.Kill();
            }
        }
    }
}
