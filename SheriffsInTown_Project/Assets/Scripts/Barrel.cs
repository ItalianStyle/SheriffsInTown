using System.Collections;
using UnityEngine;

public class Barrel : MonoBehaviour
{
    [Tooltip("Quanti secondi devono passare per far respawnare l'oggetto? (in secondi)")]
    [SerializeField] float timeToRespawn;
    MeshRenderer barrelRenderer;
    MeshCollider barrelCollider;

    private void Awake()
    {
        barrelRenderer = GetComponent<MeshRenderer>();
        barrelCollider = GetComponent<MeshCollider>();
    }

    //Chiamato dal PlayerShooting quando colpisce il barile
    public virtual void Destroy()
    {       
        //Disabilito la visuale del barile
        barrelRenderer.enabled = false;

        //Disabilito il collider fisico del barile per evitare di attivare l'effetto piu di una volta
        barrelCollider.enabled = false;

        ShootFX_Manager.PlayBigExplosionFX(transform.position);
        StartCoroutine(RespawnObject());
    }

    IEnumerator RespawnObject()
    {
        yield return new WaitForSeconds(timeToRespawn);
        //Disabilito la visuale del barile
        barrelRenderer.enabled = true;

        //Disabilito il collider fisico del barile per evitare di attivare l'effetto piu di una volta
        barrelCollider.enabled = true;
    }
}