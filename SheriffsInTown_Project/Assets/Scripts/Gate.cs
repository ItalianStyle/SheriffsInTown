using System;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public static event Action<GameObject> OnPlayerNearGate = delegate { };
    public static event Action OnPlayerLeftGate = delegate { };

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            OnPlayerNearGate?.Invoke(gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            OnPlayerLeftGate?.Invoke();
        }
    }
}
