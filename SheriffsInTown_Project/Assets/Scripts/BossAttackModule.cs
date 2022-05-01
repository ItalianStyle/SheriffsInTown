using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(EnemyHealthSystem))]
public class BossAttackModule : MonoBehaviour
{
    EnemyHealthSystem bossHealth;
    /// <summary>
    /// 3 tipi di attacchi:
    /// Proiettili a ventaglio,
    /// Salto con danni quando atterra,
    /// Lancio del barile esplosivo sulla posizione del giocatore
    /// </summary>
    public enum AttackType { Bullets, Jump, ThrowBarrel}

    [Header("Attacco tipo proiettile")]
    [Tooltip("Danni inflitti dal proiettile")]
    [SerializeField] int bulletDamage;

    [Tooltip("Quanti proiettili spawna il boss con l'attacco dei proiettili")]
    [SerializeField] int bulletAmount;

    [Tooltip("Quanto è ampio il ventaglio di proiettili")]
    [SerializeField] float bulletAttackAngle;

    [Tooltip("Forza richiesta per muovere i proiettili con AddForce()")]
    [SerializeField] float pushForce;

    [Tooltip("Il punto da dove spawnano tutti i proiettili")]
    [SerializeField] Transform bulletSpawnPointTransform;

    [Header("Salto")]
    [Tooltip("Quanto alto salta il boss")]
    [SerializeField] float jumpHeight;
    [Tooltip("Quanti danni infligge al giocatore quando atterra")]
    [SerializeField] float landingDamage;

    List<Bullet> bullets;

    private void Awake()
    {
        bossHealth = GetComponent<EnemyHealthSystem>();
    }


    private void Start()
    {
        bullets = new List<Bullet>();
    }

    public void Attack()
    {
        AttackType attackType = (AttackType) Random.Range(0, 2);
        StartAttack(AttackType.Bullets);
    }

    private void StartAttack(AttackType attackType)
    {
        switch(attackType)
        {
            case AttackType.Bullets:
                PrepareBullets();
                ShootBullets();
                break;

            case AttackType.Jump:
                Jump();
                break;

            case AttackType.ThrowBarrel:
                break;
        }
    }

    private void Jump()
    {
        throw new System.NotImplementedException();
    }

    private void ShootBullets()
    {
        for(int i = 0; i < bulletAmount; i++)
        {
            bullets[i].Fire(pushForce, bulletDamage);
        }
        bullets.Clear();
    }

    private void PrepareBullets()
    {
        //Calcolo ogni quanti gradi va ruotato un proiettile per coprire uniformemente il ventaglio
        float deltaAngle = bulletAttackAngle / bulletAmount;
        Debug.Log($"DeltaAngle: {deltaAngle}");
        //Definisco i limiti del ventaglio
        float maxAngle = bulletAttackAngle / 2;
        float minAngle = -maxAngle;
        Debug.Log($"minAngle: {minAngle}, maxAngle: {maxAngle}");

        //Parto a far lanciare proiettili dalla sinistra del boss
        float currentAngle = minAngle;
        for (int i = 0; i < bulletAmount; i++)
        {
            Debug.Log($"{i+1}° Ciclo");
            GameObject bullet = ObjectPooler.SharedInstance.GetPooledObject("Bullet");
            //Posiziona il proiettile
            bullet.transform.position = bulletSpawnPointTransform.position;
            Debug.Log($"Posizionato il proiettile_{i} nella posizione globale -> {bullet.transform.position}");
            //Ruota il proiettile
            Debug.Log($"Il proiettile prima di applicare la rotazione di {currentAngle}° ha questa rotazione globale -> {transform.rotation.eulerAngles}");
            bullet.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + currentAngle, 0);
            Debug.Log($"Il proiettile_{i} ha ora questa rotazione globale -> {bullet.transform.rotation.eulerAngles}");

            bullet.SetActive(true);
            //Preparo l'angolo per il prossimo proiettile
            currentAngle += deltaAngle;
            
            currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
            Debug.Log($"Il prossimo proiettile deve ruotare di {currentAngle}°");
            //Aggiungo il proiettile alla lista per lanciarlo dopo
            bullets.Add(bullet.GetComponent<Bullet>());
        }
    }
}