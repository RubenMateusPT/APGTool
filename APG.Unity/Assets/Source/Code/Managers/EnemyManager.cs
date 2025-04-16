using System;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private Enemy[] _enemies;

    private void Awake()
    {
        _enemies = FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    private void Start()
    {
        foreach (var enemy in _enemies)
        {
            enemy.RandomizeDirection();
            enemy.RandomizeMovementSpeed();
        }
    }
}
