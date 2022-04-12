using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RespawnManager : MonoBehaviour
{
    public static event Action OnPlayerRespawned = delegate { };

    public static RespawnManager Instance;

    GameObject[] spawnPoints;  //Lista di tutti gli spawn point della mappa
    Transform spawnPoint = null;   //Il punto di spawn del player

    void Start()
    {
        Instance = this;

        //Inizializzo la lista dei punti di spawn della mappa
        spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
    }

    void Update()
    {
        
    }

    public void RespawnPlayer(GameObject player)
    {
        //Seleziona un punto di spawn casuale
        spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length - 1)].transform;
        //Posiziona il player
        player.transform.position = spawnPoint.position;

        player.SetActive(true);
        OnPlayerRespawned?.Invoke();
    }
}
