using UnityEngine;


public class KeyHolder : MonoBehaviour
{
    
    BridgeLever levaInventory;

    struct BridgeLever
    {
        bool isLevaPossessed;
        bool isPomelloPossessed;
        bool isBasePossessed;
    }

    int possessedKeys = 0;
    bool canListenInput = false;
    
    // if (AbbassaPonte.levaPresente&&

    GameObject gateToOpen;

    private void Start()
    {
        Gate.OnPlayerNearGate += HandlePlayerNearGate;
        Gate.OnPlayerLeftGate += HandlePlayerLeftGate;
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
        Gate.OnPlayerNearGate -= HandlePlayerNearGate;
        Gate.OnPlayerLeftGate -= HandlePlayerLeftGate;
    }

    void HandlePlayerNearGate(GameObject gate)
    {
        canListenInput = true;
        gateToOpen = gate;
    }

    void HandlePlayerLeftGate() 
    {
        canListenInput = false;
        gateToOpen = null;
    }

    public void TakeKey() => possessedKeys++;

    public void UseKey() => possessedKeys--;
}
