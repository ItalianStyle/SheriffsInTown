using System;
using UnityEngine;

namespace SheriffsInTown
{
    public class BossTrigger : MonoBehaviour
    {
        public static event Action<Collider> OnPlayerEnteredBossArea = delegate { };

        BoxCollider[] walls;
        private void Awake()
        {
            walls = GetComponentsInChildren<BoxCollider>();
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                OnPlayerEnteredBossArea?.Invoke(other);
                //Attiva i muri
                TriggerWalls();

                //Setta il nuovo checkpoint
            }
        }

        void TriggerWalls()
        {
            foreach (BoxCollider wall in walls)
            {
                wall.enabled = true;
            }
        }
    }
}