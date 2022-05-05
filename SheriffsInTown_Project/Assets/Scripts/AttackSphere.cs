using System.Collections;
using UnityEngine;

namespace SheriffsInTown
{
    public class AttackSphere : MonoBehaviour
    {
        PlayerHealthSystem playerHealthSystem;

        public void TriggerBomb(float timeToExplode, int explosionDamage)
        {
            StartCoroutine(StartExplosionCountdown(timeToExplode, explosionDamage));
        }

        IEnumerator StartExplosionCountdown(float timeToExplode, int explosionDamage)
        {
            float timer = timeToExplode;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                yield return null;
            }
            if (playerHealthSystem)
            {
                playerHealthSystem.CurrentHealth -= explosionDamage;
            }

            gameObject.SetActive(false);
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
    }
}