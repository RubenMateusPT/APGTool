using UnityEngine;
using UnityEngine.Events;

public class RequestTrigger : MonoBehaviour
{
    public UnityEvent OnTriggerEnter = new UnityEvent();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 7)
        {
            OnTriggerEnter.Invoke();
            this.gameObject.SetActive(false);
        }
    }
}
