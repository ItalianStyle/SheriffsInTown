//Source: https://www.raywenderlich.com/847-object-pooling-in-unity#toc-anchor-002

using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Sistema di Object pooling per gli oggetti da lanciare
/// </summary>

//Classe per il singolo oggetto copiabile
[System.Serializable]
public class ObjectPoolItem
{
    public GameObject objectToPool;     //Il vero e proprio oggetto da rendere copiabile
    public int amountToPool;    //Quantità iniziale da instanziare
    public bool shouldExpand = true;     //Flag per rendere la lista espandibile a runtime
}

//Classe contenente una istanza pubblica e statica per creare più liste di oggetti identici
public class ObjectPooler : MonoBehaviour
{
    public List<ObjectPoolItem> itemsToPool;     //Lista di oggetti da copiare
    public List<GameObject> pooledObjects;      //Lista dell'oggetto e delle sue copie
    public static ObjectPooler SharedInstance;      //Istanza per accedere agli oggetti del pooler

    private void Awake()
    {
        SharedInstance = this;
    }

    //Si crea una lista di oggetti copia per ogni oggetto da poolizzare
    void Start()
    {
        pooledObjects = new List<GameObject>();
        foreach (ObjectPoolItem item in itemsToPool)
        {
            for (int i = 0; i < item.amountToPool; i++)
            {
                GameObject obj = (GameObject)Instantiate(item.objectToPool, Vector3.zero, Quaternion.identity);
                obj.SetActive(false);
                pooledObjects.Add(obj);
            }
        }
    }

    //Ritorna il primo oggetto copia utilizzabile, eventualmente se il flag è attivo espande la rispettiva lista ed instanzia una nuova copia
    public GameObject GetPooledObject(string tag)
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy && pooledObjects[i].CompareTag(tag))
            {
                return pooledObjects[i];
            }
        }
        foreach (ObjectPoolItem item in itemsToPool)
        {
            if (item.objectToPool.CompareTag(tag))
            {
                if (item.shouldExpand)
                {
                    GameObject obj = (GameObject)Instantiate(item.objectToPool, Vector3.zero, Quaternion.identity);
                    obj.SetActive(false);
                    pooledObjects.Add(obj);
                    return obj;
                }
            }
        }
        return null;
    }
}