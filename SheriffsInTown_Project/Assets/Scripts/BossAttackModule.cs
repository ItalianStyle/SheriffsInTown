using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace SheriffsInTown
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Rigidbody))]
    public class BossAttackModule : MonoBehaviour
    {
        /// <summary>
        /// 3 tipi di attacchi:
        /// Proiettili a ventaglio,
        /// Salto con danni quando atterra,
        /// Lancio del barile esplosivo sulla posizione del giocatore
        /// </summary>
        public enum AttackType { Bullets, Jump, ThrowBarrel }

        public static event Action<int, float> OnBossLanded = delegate { };

        [Header("Attacco Proiettile")]
        [Tooltip("Danni inflitti dal proiettile")]
        [SerializeField] int bulletDamage;

        [Tooltip("Quanti proiettili spawna il boss con l'attacco dei proiettili")]
        [SerializeField] int bulletAmount;

        [Tooltip("Quanto è ampio il ventaglio di proiettili")]
        [SerializeField] float bulletAttackAngle;

        [Tooltip("Forza richiesta per muovere i proiettili con AddForce()")]
        [SerializeField] float pushForce;

        [Tooltip("Quanto tempo deve passare prima di disattivare il proiettile se non incontra ostacoli")]
        [SerializeField] float bulletLifeTime;

        [Tooltip("Il punto da dove spawnano tutti i proiettili")]
        [SerializeField] Transform bulletSpawnPointTransform;

        [Header("Attacco Salto")]
        [Tooltip("Forza di salto del boss")]
        [Space] [SerializeField] float jumpForce;
        [Tooltip("Quanti danni infligge al giocatore quando atterra")]
        [SerializeField] int landingDamage;
        [Tooltip("Per quanto tempo il giocatore rimane paralizzato")]
        [SerializeField] float stunTime;

        [Header("Attacco Barile")]
        [SerializeField] int barrelNumber;
        [SerializeField] float maxDistanceFromPlayer;

        bool isJumping = false;

        List<Bullet> bullets;
        NavMeshAgent bossNavMeshAgent;
        Rigidbody bossRigidbody;

        Transform playerTransform;
        
        private void Awake()
        {
            bossNavMeshAgent = GetComponent<NavMeshAgent>();
            bossRigidbody = GetComponent<Rigidbody>();
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        private void Start()
        {
            bullets = new List<Bullet>();
            isJumping = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            //Se il boss collide col terreno
            if (collision.collider != null && collision.collider.CompareTag("Ground"))
            {
                if (isJumping)
                {
                    isJumping = false;
                    CanPrepareBossForJump(isJump: false, collision.GetContact(0).point);
                    
                    OnBossLanded?.Invoke(landingDamage, stunTime);             
                }
            }
        }

        //Meccanica di salto con il NavMesh Agent: https://stackoverflow.com/questions/66007738/unity-how-to-jump-using-a-navmeshagent-and-click-to-move-logic
        private void Jump()
        {
            CanPrepareBossForJump(isJump: true);
            bossRigidbody.AddRelativeForce(Vector3.up * jumpForce, ForceMode.Impulse);
            StartCoroutine(SetIsJumping());  
        }

        //Setto con un piccolo ritardo isJumping = true per non far partire OnCollisionEnter subito
        IEnumerator SetIsJumping()
        {
            yield return new WaitForSeconds(.1f);
            isJumping = true;
        }

        private void CanPrepareBossForJump(bool isJump, Vector3 contactPoint = new Vector3())
        {
            if (bossNavMeshAgent.enabled)
            {
                if (isJump)
                    // set the agents target to where you are before the jump
                    // this stops her before she jumps. Alternatively, you could
                    // cache this value, and set it again once the jump is complete
                    // to continue the original move
                    bossNavMeshAgent.SetDestination(transform.position);
                
                else
                    //Per evitare che il boss snappi alla posizione del navMeshAgent (che viene simulato e non quello visibile)
                    //https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent-nextPosition.html
                    bossNavMeshAgent.nextPosition = contactPoint;

                // disable the agent
                bossNavMeshAgent.updatePosition = !isJump;
                bossNavMeshAgent.updateRotation = !isJump;
                bossNavMeshAgent.isStopped = isJump;
            }
            // make the jump
            bossRigidbody.isKinematic = !isJump;
            bossRigidbody.useGravity = isJump;
        }

        public void Attack()
        {
            AttackType attackType = (AttackType)Random.Range(0, 2);
            StartAttack(attackType);
        }

        private void StartAttack(AttackType attackType)
        {
            switch (attackType)
            {
                case AttackType.Bullets:
                    PrepareBullets();
                    ShootBullets();
                    break;

                case AttackType.Jump:
                    Jump();
                    break;

                case AttackType.ThrowBarrel:
                    SpawnAttackColliders();
                    break;
            }
        }

        private void SpawnAttackColliders()
        {
            //Prepara la sfera
            for(int i = 0; i < barrelNumber; i++)
            {
                GameObject attackingSphere = ObjectPooler.SharedInstance.GetPooledObject("AttackSphere");
                float randomAngle = Random.Range(0f, 359.9f);
                float randomDistanceFromPlayer = Random.Range(0f, maxDistanceFromPlayer);
                Vector2 randomDirection = new Vector2(Mathf.Cos(Mathf.Deg2Rad * randomAngle), Mathf.Sin(Mathf.Deg2Rad * randomAngle)).normalized;

                Vector3 randomPosition = new Vector3(randomDirection.x, 0, randomDirection.y) * randomDistanceFromPlayer;
                randomPosition += playerTransform.position;
                if (Physics.Raycast(randomPosition, -Vector3.up, out RaycastHit info, Mathf.Infinity))
                {
                    //Stabilisci la posizione dove spawnare l'area
                    attackingSphere.transform.position = info.point + (Vector3.up * .1f);
                    
                    attackingSphere.SetActive(true);
                }
            }
            
        }
        private void ShootBullets()
        {
            //Per ogni proiettile spawnato
            for (int i = 0; i < bulletAmount; i++)
            {
                //Spara l'i-esimo proiettile passandogli la forza di spinta, il danno ed il tempo di vita
                bullets[i].Fire(pushForce, bulletDamage, bulletLifeTime);
            }

            //Pulisci la lista di proiettili sparati per la prossima salva
            bullets.Clear();
        }

        private void PrepareBullets()
        {
            float deltaAngle = bulletAttackAngle / bulletAmount;    //Calcolo ogni quanti gradi va ruotato un proiettile per coprire uniformemente il ventaglio

            float maxAngle = bulletAttackAngle / 2;     //Definisco il limite destro del ventaglio (guardando dal punto di vista del boss)
            float minAngle = -maxAngle;     //Definisco il limite sinistro del ventaglio (guardando dal punto di vista del boss)

            float currentAngle = minAngle;      //Parto a far lanciare proiettili dalla sinistra del boss
            for (int i = 0; i < bulletAmount; i++)
            {
                GameObject bullet = ObjectPooler.SharedInstance.GetPooledObject("Bullet");

                bullet.transform.position = bulletSpawnPointTransform.position;     //Posiziona il proiettile
                bullet.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + currentAngle, 0);    //Ruota il proiettile
                bullet.SetActive(true);

                currentAngle += deltaAngle;     //Preparo l'angolo per il prossimo proiettile
                currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);   //Lo limito entro il ventaglio per sicurezza
                bullets.Add(bullet.GetComponent<Bullet>());     //Aggiungo il proiettile alla lista per lanciarlo dopo
            }
        }
    }
}