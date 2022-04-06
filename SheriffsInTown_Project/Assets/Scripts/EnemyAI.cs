using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Tooltip("A che distanza dal giocatore attacca")]
    [SerializeField] float attackDistance;

    [Tooltip("Tempo di ricarica prima di riattaccare")]
    [SerializeField] float attackReloadTime = 1f;

    [Tooltip("Danno a colpo che infligge il nemico al giocatore")]
    [SerializeField] int damage = 20;
    
    bool playerDetected = false;
    PlayerHealthSystem targetHealth = null;
    NavMeshAgent agent = null;
    bool canShoot;
    Material enemyMaterial;
    Color initialColor;

    private void Start()
    {
        enemyMaterial = GetComponent<Renderer>().material;
        initialColor = enemyMaterial.color;
        agent = GetComponent<NavMeshAgent>();
        canShoot = true;
    }

    private void Update()
    {
        if (!playerDetected || !targetHealth)
            return;
        
        if (Vector3.Distance(targetHealth.transform.position, transform.position) <= attackDistance)
        {
            agent.isStopped = true;
            if (canShoot)
            {
                //attack
                targetHealth.SetCurrentHealth(damage, true);
                StartCoroutine(Reload());
            }
        }
        else
        {
            agent.isStopped = false;
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
            enemyMaterial.color = Color.green;
            targetHealth = other.GetComponent<PlayerHealthSystem>();
            Debug.Log("Il nemico mi vede");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerDetected = false;
            enemyMaterial.color = initialColor;
            targetHealth = null;
            Debug.Log("Il nemico mi ha perso di vista");
        }
    }
}
