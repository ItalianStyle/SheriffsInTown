using System.Collections;
using UnityEngine;

public class Barrel : MonoBehaviour
{
    [SerializeField] ParticleSystem explosionFX;

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
        //Crea l'effetto dell'esplosione
        Instantiate(explosionFX, transform);
        
        //Disabilito la visuale del barile
        barrelRenderer.enabled = false;

        //Disabilito il collider fisico del barile per evitare di attivare l'effetto piu di una volta
        barrelCollider.enabled = false;

        explosionFX.Play();
        //Distruggi il barile dopo l'effetto dell'esplosione
        StartCoroutine(DestroyObjectAfterDelay(explosionFX.main.duration));
    }

    IEnumerator DestroyObjectAfterDelay(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        Destroy(gameObject);
    }
}