using UnityEngine;

public class KeyHolder : MonoBehaviour
{
    int possessedKeys = 0;
    bool canListenInput = false;

    GameObject gateToOpen;

    private void Start()
    {
        Gate.OnPlayerNearGate += (gate) =>
        {
            canListenInput = true;
            gateToOpen = gate;
        };

        Gate.OnPlayerLeftGate += () =>
        {
            canListenInput = false;
            gateToOpen = null;
        };
    }

    private void Update()
    {
        if (possessedKeys > 0 && canListenInput && Input.GetKeyDown(KeyCode.E))
        {
            gateToOpen.SetActive(false);
            UseKey();
        }
    }

    private void OnDisable()
    {
        Gate.OnPlayerNearGate -= (gate) =>
        {
            canListenInput = true;
            gateToOpen = gate;
        };

        Gate.OnPlayerLeftGate -= () =>
        {
            canListenInput = false;
            gateToOpen = null;
        };
    }

    public void TakeKey() => possessedKeys++;

    public void UseKey() => possessedKeys--;
}
