using System;
using APG.Unity.Managers;
using UnityEngine;
using UnityEngine.Events;

public class checkpoint : MonoBehaviour
{
    public GameObject spawnpoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 7)
        {
            spawnpoint.transform.position = transform.position;
            FindFirstObjectByType<APGManager>().SendScreenShoot($"Player reached {gameObject.name}");
            this.gameObject.SetActive(false);
        }
            
    }
}
