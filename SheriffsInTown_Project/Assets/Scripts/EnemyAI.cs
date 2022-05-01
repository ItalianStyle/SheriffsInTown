using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public static event Action OnPlayerEnteredBossArea = delegate { };

    public enum EnemyType { Normal, Boss }
    [Tooltip("In base a questo enum cambia il comportamento del nemico")]
    public EnemyType enemyType;

    [Tooltip("A che distanza dal giocatore attacca")]
    [SerializeField] float attackDistance;

    [Tooltip("Tempo di ricarica prima di riattaccare")]
    [SerializeField] float attackReloadTime = 1f;

    [Tooltip("Danno a colpo che infligge il nemico al giocatore.\nSi applica solo al nemico normale!\nOgni attacco speciale del boss ha il suo danno dedicato.")]
    [SerializeField] int damage = 20;

    [Tooltip("Cosa può colpire il nemico con il raycast?")]
    [SerializeField] LayerMask layerMask;

    [SerializeField] Transform muzzleDX_Transform;
    [SerializeField] Transform muzzleSX_Transform;

    bool playerDetected = false;
    PlayerHealthSystem playerHealth = null;
    NavMeshAgent agent = null;
    bool canShoot;

    Material enemyMaterial;
    Color initialColor;

    BossAttackModule bossAttacks;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyMaterial = transform.Find("Mesh").GetComponent<Renderer>().material;

        if (enemyType is EnemyType.Boss)
            bossAttacks = GetComponent<BossAttackModule>();
    }
    private void Start()
    {

        initialColor = enemyMaterial.color;

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
        //Se il giocatore non è dentro l'area trigger non fare niente
        if (!(playerDetected && playerHealth))
            return;

        //Debug.Log(Vector3.Distance(playerHealth.transform.position, transform.position));
        if (Vector3.Distance(playerHealth.transform.position, transform.position) <= attackDistance)
        {
            agent.isStopped = true;
            if (canShoot)
            {
                if (enemyType is EnemyType.Normal)
                {
                    ShootFX_Manager.PlayMuzzleFlashFX(muzzleDX_Transform);
                    Ray ray = new Ray
                    {
                        origin = transform.position,
                        direction = playerHealth.transform.position - transform.position
                    };

                    if (Physics.Raycast(ray, out RaycastHit hitInfo, attackDistance, layerMask))//attack
                    {
                        ShootFX_Manager.PlayBulletHitFX(hitInfo.point);

                        if (hitInfo.collider.CompareTag("Player"))
                        {
                            playerHealth.CurrentHealth -= damage;
                        }
                    }                
                }
                else
                {
                    //Se il nemico è il boss
                    bossAttacks.Attack();
                }
                //Ricarica quando finisce l'attacco
                StartCoroutine(Reload());
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(playerHealth.transform.position);
            }
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
            if (enemyType is EnemyType.Boss)
                OnPlayerEnteredBossArea?.Invoke();
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