using System;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    //0 -> pistola sinistra 1 -> pistola destra
    MeshRenderer[] gunRenderers = new MeshRenderer[2];
    
    //Riferimento al cappello del giocatore
    MeshRenderer hatRenderer;

    [SerializeField] Material goldGunMaterial;

    private void Awake()
    {
        gunRenderers[0] = transform.Find("Mesh_Corpo/PistolaSX").GetComponent<MeshRenderer>();
        gunRenderers[1] = transform.Find("Mesh_Corpo/PistolaDX").GetComponent<MeshRenderer>();

        hatRenderer = transform.Find("Mesh_Corpo/Mesh_CappelloSceriffo").GetComponent<MeshRenderer>();

        PlayerShooting.OnPlayerChangedFireMode += SetGunsVisibility;
        Pickup.OnHatPickupTaken += ShowHat;
        Pickup.OnGoldGunPickupTaken += ShowGoldGuns;
    }
    private void ShowGoldGuns()
    {
        gunRenderers[0].material = goldGunMaterial;
        gunRenderers[1].material = goldGunMaterial;
    }

    void Start()
    {
        Debug.LogWarning("Start Chiamato");
        hatRenderer.enabled = false;
    }

    private void OnDestroy()
    {
        PlayerShooting.OnPlayerChangedFireMode -= SetGunsVisibility;
        Pickup.OnHatPickupTaken -= ShowHat;

        gunRenderers[0] = null;
        gunRenderers[1] = null;

        hatRenderer = null;
    }

    private void ShowHat(float newMovementSpeed, float newRunMovementSpeed)
    {
        hatRenderer.enabled = true;
    }

    private void SetGunsVisibility(bool isDoubleGunMode)
    {
        //Fai apparire la pistola (sulla mano sinistra) se la modalita di sparo e' doppia
        gunRenderers[0].enabled = isDoubleGunMode;
        //Faccio apparire in ogni caso la pistola della mano destra
        gunRenderers[1].enabled = true;
    }
}