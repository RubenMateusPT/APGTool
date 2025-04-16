using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class SimpleTrigger : MonoBehaviour
{
    [SerializeField]
    private string tagToCompare;

    [SerializeField]
    private bool runOnce = true;

    [SerializeField]
    UnityEvent onTriggerEnter = new UnityEvent();

    private bool _hasBeenTriggered;

    private void Start()
    {
        _hasBeenTriggered = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(_hasBeenTriggered)
            return;

        if (other.gameObject.CompareTag(tagToCompare))
        {
            if (runOnce)
                _hasBeenTriggered = true;
            onTriggerEnter.Invoke();
        }
    }
}
