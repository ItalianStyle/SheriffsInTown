using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SheriffsInTown
{
    public class RespawnManager : MonoBehaviour
    {
        public static event Action OnPlayerRespawned = delegate { };

        public static RespawnManager Instance;

        GameObject[] spawnPoints;  //Lista di tutti gli spawn point della mappa
        GameObject lastCheckpointSaved;     //L'ultimo checkpoint salvato dal player

        private void Awake()
        {
            Instance = this;
            //Inizializzo la lista dei punti di spawn della mappa
            spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
        }
        void Start()
        {
            lastCheckpointSaved = spawnPoints[0];
            CheckpointTrigger.OnPlayerEnteredCheckpointTrigger += SaveCheckpoint;
        }

        private void OnDestroy()
        {
            CheckpointTrigger.OnPlayerEnteredCheckpointTrigger -= SaveCheckpoint;
        }

        private void SaveCheckpoint(GameObject checkpoint)
        {
            //Trova il checkpoint passato nella lista
            foreach(GameObject spawnPoint in spawnPoints)
            {
                if(spawnPoint.name == checkpoint.name)
                {
                    lastCheckpointSaved = checkpoint;
                    Debug.Log("Checkpoint salvato con successo");
                    break;
                }
            }
        }

        public void RespawnPlayer(GameObject player)
        {
            //Posiziona il player
            player.transform.position = lastCheckpointSaved.transform.position;

            player.SetActive(true);
            OnPlayerRespawned?.Invoke();
        }
    }
}