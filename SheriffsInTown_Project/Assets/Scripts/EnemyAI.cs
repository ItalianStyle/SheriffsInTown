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

    [Tooltip("Indica se questo nemico è il boss")]
    [SerializeField] bool isBoss;

    bool playerDetected = false;
    PlayerHealthSystem playerHealth = null;
    NavMeshAgent agent = null;
    bool canShoot;
    Material enemyMaterial;
    Color initialColor;

    private void Start()
    {
        enemyMaterial = transform.Find("Mesh").GetComponent<Renderer>().material;
        initialColor = enemyMaterial.color;
        agent = GetComponent<NavMeshAgent>();
        canShoot = true;

        PlayerHealthSystem.OnPlayerDead += HandlePlayerDeath;
        GameStateManager.Instance.OnGameStateChanged += HandleEnemyBehaviour;
    }

    private void HandlePlayerDeath(GameObject player)
    {
        //Non inseguire il giocatore
        EngagePlayer(false);
        //Smetti di ricaricare prima di sparare
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        PlayerHealthSystem.OnPlayerDead -= HandlePlayerDeath;
        GameStateManager.Instance.OnGameStateChanged -= HandleEnemyBehaviour;
    }

    private void Update()
    {
        if (!(playerDetected && playerHealth))
            return;
        
        if (Vector3.Distance(playerHealth.transform.position, transform.position) <= attackDistance)
        {
            agent.isStopped = true;
            if (canShoot)
            {
                Ray ray = new Ray();
                ray.origin = transform.position;
                ray.direction = playerHealth.transform.position - transform.position;

                //if(Physics.Raycast(ray, attackDistance))//attack
                playerHealth.CurrentHealth -= damage;
                StartCoroutine(Reload());
            }
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(playerHealth.transform.position);
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
            EngagePlayer(true, other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EngagePlayer(false, other);
        }
    }

    private void HandleEnemyBehaviour(GameState newGameState)
    {
        agent.isStopped = !(newGameState is GameState.Gameplay);
        enabled = newGameState is GameState.Gameplay;
    }

    private void EngagePlayer(bool canChasePlayer, Collider other = null)
    {
        playerDetected = canChasePlayer;
        enemyMaterial.color = canChasePlayer ? Color.green : initialColor;
        playerHealth = canChasePlayer ? other.GetComponent<PlayerHealthSystem>() : null;
    }
}