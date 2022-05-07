using System.Collections;
using UnityEngine;

namespace SheriffsInTown
{
    public class AttackSphere : MonoBehaviour
    {
        PlayerHealthSystem playerHealthSystem;
        GameObject barrel;
        [SerializeField] float initialBarrelHeight;
        [SerializeField] float initialScale;
        [SerializeField] float finalScale;
        [SerializeField] float timeToExplode;
        [SerializeField] int explosionDamage;
        
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1,0,0,.5f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(transform.position, .5f);
        }

        private void OnEnable()
        {
            barrel = ObjectPooler.SharedInstance.GetPooledObject("FxTemporaire");
            StartCoroutine(StartAreaAnimation());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerHealthSystem = other.GetComponent<PlayerHealthSystem>();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerHealthSystem = null;
            }
        }

        IEnumerator StartAreaAnimation()
        {
            Vector3 initialPosition = transform.position + Vector3.up * initialBarrelHeight;
            barrel.SetActive(true);
            for(float time = 0f; time < timeToExplode; time += Time.deltaTime)
            {
                float progress = time / timeToExplode;
                transform.localScale = Vector3.Lerp(Vector3.one * initialScale, Vector3.one * finalScale, progress);
                barrel.transform.position = Vector3.Lerp(initialPosition, transform.position, progress);
                yield return null;
            }
            StartCoroutine(HoldScale());
        }

        IEnumerator HoldScale()
        {
            yield return new WaitForSeconds(1);
            Explode();
        }

        void Explode()
        {
            if (playerHealthSystem)
            {
                playerHealthSystem.CurrentHealth -= explosionDamage;
            }
            playerHealthSystem = null;
            ShootFX_Manager.PlayBigExplosionFX(transform.position);
            barrel.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}