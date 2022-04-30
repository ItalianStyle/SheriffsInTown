using System;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [Tooltip("Inserire qui le due pistole:\n0 -> pistola sinistra\n1 -> pistola destra")]
    [SerializeField] MeshRenderer[] gunRenderers = new MeshRenderer[2];

    [Tooltip("Inserisci qui il cappello")]
    [SerializeField] MeshRenderer hatRenderer;

    [SerializeField] Material goldGunMaterial;

    private void OnEnable()
    {
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
        hatRenderer.enabled = false;
    }

    private void OnDestroy()
    {
        PlayerShooting.OnPlayerChangedFireMode -= SetGunsVisibility;
        Pickup.OnHatPickupTaken -= ShowHat;
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