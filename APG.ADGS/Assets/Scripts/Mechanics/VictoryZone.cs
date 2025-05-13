using System.Collections;
using APG.Unity.Managers;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Marks a trigger as a VictoryZone, usually used to end the current game level.
    /// </summary>
    public class VictoryZone : MonoBehaviour
    {

        void OnTriggerEnter2D(Collider2D collider)
        {
            var p = collider.gameObject.GetComponent<PlayerController>();
            if (p != null)
            {
                var ev = Schedule<PlayerEnteredVictoryZone>();
                ev.victoryZone = this;

                StopAllCoroutines();
                StartCoroutine(SendShot());
            }
        }

        private IEnumerator SendShot()
        {
            yield return new WaitForSeconds(1);
            FindFirstObjectByType<APGManager>().SendScreenShoot("Player has finished level!");
        }
    }
}