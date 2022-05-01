using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [SerializeField] GameObject oggettoDaCollezionare;
    [SerializeField] int grandezzaPool;
    [SerializeField] bool ingrandisciPool;
    List<GameObject> pool;
    public static ObjectPooler Istanza;

    private void Awake()
    {
        Istanza = this;
    }

    private void Start()
    {
        pool = new List<GameObject>(); // Inizializza la lista degli oggetti da utilizzare
        for (int i = 0; i < grandezzaPool; i++) // Istanzia tutte le copie nella scena
        {
            IstanziaOggettoDelPool();
        }
    }

    private GameObject IstanziaOggettoDelPool()
    {
        GameObject tempObject = Instantiate(oggettoDaCollezionare);
        tempObject.SetActive(false);
        pool.Add(tempObject);
        return tempObject;
    }

    public GameObject PrendiOggettoDalPool()
    {
        if (pool.Count > 0) // Se la lista degli oggetti da utilizzare non � vuota
        {
            for (int i = 0; i < pool.Count; i++) // Cicla nella lista di oggetti da utilizzare
            {
                if (!pool[i].activeInHierarchy) // Se l'i-esimo oggetto � disattivo
                    return pool[i]; // Restituiscilo al chiamante
            }
            // A questo punto del codice non sono stati trovati oggetti liberi da riutilizzare
            if (ingrandisciPool) // Verifica se � possibile espandere la lista
            {
                GameObject obj = IstanziaOggettoDelPool(); //Crea una nuova copia da aggiungere alla lista di oggetti
                return obj; //Restituiscilo al chiamante
            }
        }
        return null; // Se la lista � vuota o se non ci sono oggetti disponibili (senza la possibilit� di espandere la lista) restituisci niente al chiamante
    }
}