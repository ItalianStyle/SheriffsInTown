using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace SheriffsInTown
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        public enum EnemyType { Normal, Boss }
        [Tooltip("In base a questo enum cambia il comportamento del nemico")]
        public EnemyType enemyType;

        [Header("Nemico normale")]
        [Tooltip("A che distanza dal giocatore il bandito normale attacca")]
        [SerializeField] float normalAttackDistance;
        [Tooltip("Danno a colpo che infligge il nemico al giocatore.\nSi applica solo al nemico normale!\nOgni attacco speciale del boss ha il suo danno dedicato.")]
        [SerializeField] int normalEnemyDamage = 20;

        [Header("Generale")]
        [Tooltip("Tempo di ricarica prima di riattaccare")]
        [SerializeField] float attackReloadTime = 1f;

        

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
            CheckpointTrigger.OnPlayerEnteredBossArea += HandleBossBehaviour;
        }

        private void HandleBossBehaviour(Collider playerCollider)
        {
            if (enemyType is EnemyType.Boss)
            {
                EngagePlayer(true, playerCollider);
                StartCoroutine(Reload());
            }
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
            CheckpointTrigger.OnPlayerEnteredBossArea -= HandleBossBehaviour;
        }

        private void Update()
        {
            //Se il giocatore non è dentro l'area trigger non fare niente
            if (!(playerDetected && playerHealth))
                return;

            //Altrimenti guarda sempre il giocatore
            LookAtOnlyYAxis();

            if (canShoot)
            {
                if (enemyType is EnemyType.Normal)
                {
                    if (Vector3.Distance(playerHealth.transform.position, transform.position) <= normalAttackDistance)
                    {
                        agent.isStopped = true;
                        ShootFX_Manager.PlayMuzzleFlashFX(muzzleDX_Transform);
                        Ray ray = new Ray
                        {
                            origin = transform.position,
                            direction = playerHealth.transform.position - transform.position
                        };

                        if (Physics.Raycast(ray, out RaycastHit hitInfo, normalAttackDistance, layerMask))//attack
                        {
                            ShootFX_Manager.PlayBulletHitFX(hitInfo.point);

                            if (hitInfo.collider.CompareTag("Player"))
                            {
                                playerHealth.CurrentHealth -= normalEnemyDamage;
                            }
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
                else
                {                   
                    //Se il nemico è il boss
                    bossAttacks.Attack();
                    //Ricarica quando finisce l'attacco
                    StartCoroutine(Reload());
                }
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(playerHealth.transform.position);
            }
        }

        private void LookAtOnlyYAxis()
        {
            var lookPos = playerHealth.transform.position - transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 50);
        }

        IEnumerator Reload()
        {
            canShoot = false;
            yield return new WaitForSeconds(attackReloadTime);
            canShoot = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
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
            if(agent.enabled)
                agent.isStopped = !(newGameState is GameState.Gameplay);
            enabled = newGameState is GameState.Gameplay;
        }

        private void EngagePlayer(bool canChasePlayer, Collider other = null)
        {
            playerDetected = canChasePlayer;
            enemyMaterial.color = canChasePlayer ? Color.red : initialColor;
            playerHealth = canChasePlayer ? other.GetComponent<PlayerHealthSystem>() : null;
        }
    }
}