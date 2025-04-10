using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    private List<Enemy> enemies = new List<Enemy>();

    private void Awake()
    {
        enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None).OrderBy(e => e.gameObject.name).ToList();
    }

    public void KillEnemy(int id)
    {
        if (id < 0)
        {
            KillRandomEnemy();
        }
        else
        {
            KillTargetEnemy(id);
        }
    }

    public void KillRandomEnemy()
    {
        
        var rand = Random.Range(0, enemies.Count);
        KillTargetEnemy(rand);
    }

    public void KillTargetEnemy(int target)
    {

        var enemy = enemies.ElementAt(target);
        enemies.RemoveAt(target);
        enemy.Die();
    }
}
