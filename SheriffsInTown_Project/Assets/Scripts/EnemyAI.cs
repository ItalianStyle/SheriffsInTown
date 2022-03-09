using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Tooltip("Distanza massima alla quale il nemico inizia ad inseguire il giocatore")]
    [SerializeField] float chaseDistance;

    [Tooltip("A che distanza dal giocatore attacca")]
    [SerializeField] float attackDistance;

    [SerializeField] float attackReloadTime = 1f;
    
    bool playerDetected = false;
    PlayerHealthSystem targetHealth = null;
    NavMeshAgent agent = null;
    bool canShoot;
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        canShoot = true;
    }

    private void Update()
    {
        if (playerDetected && targetHealth)
        {
            if(canShoot && Vector3.Distance(targetHealth.transform.position, transform.position) <= attackDistance)
            {
                //attack
                targetHealth.SetCurrentHealth(20, true);
                StartCoroutine(Reload());
            }
            //transform.LookAt(target);
            agent.SetDestination(targetHealth.transform.position);
        }
    }

    IEnumerator Reload()
    {
        canShoot = false;
        yield return new WaitForSeconds(attackReloadTime);
        canShoot = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerDetected = true;
            targetHealth = other.GetComponent<PlayerHealthSystem>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerDetected = false;
            targetHealth = null;
        }
    }
}
