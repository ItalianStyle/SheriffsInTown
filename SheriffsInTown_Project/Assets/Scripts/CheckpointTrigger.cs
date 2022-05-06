using System;
using UnityEngine;
using UnityEditor;

namespace SheriffsInTown
{
    [ExecuteInEditMode]
    public class CheckpointTrigger : MonoBehaviour
    {
        public static event Action<Collider> OnPlayerEnteredBossArea = delegate { };
        public static event Action<GameObject> OnPlayerEnteredCheckpointTrigger = delegate { };

        [SerializeField] GameObject connectedCheckpoint;

        bool IsBossVillage => gameObject.CompareTag("BossTrigger");

        BoxCollider[] walls;

        //Come ruotare i gizmo insieme ai gameobject:
        //https://medium.com/nicholasworkshop/how-to-rotate-gizmos-to-fit-a-game-object-in-unity-fadc97e1e9de
        private void OnDrawGizmos()
        {
            Collider boxCollider = GetComponent<BoxCollider>();
            Collider sphereCollider = GetComponent<SphereCollider>();

            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.matrix = transform.localToWorldMatrix;
            if (boxCollider)
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
            else if (sphereCollider)
                Gizmos.DrawSphere(Vector3.zero, .5f);
        }

        private void Awake()
        {
            if(IsBossVillage)
                walls = GetComponentsInChildren<BoxCollider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log($"Player nel collider di {transform.parent.name}");
                if (IsBossVillage)
                {
                    OnPlayerEnteredBossArea?.Invoke(other);
                    //Attiva i muri
                    TriggerWalls();

                    //Setta il nuovo checkpoint
                }
                else
                {
                    OnPlayerEnteredCheckpointTrigger?.Invoke(connectedCheckpoint);
                }
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