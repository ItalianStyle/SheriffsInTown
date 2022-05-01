using System;
using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
public class EnemyHealthSystem : MonoBehaviour
{
    public event Action OnEnemyDead = delegate { };
    public event Action<int, int> OnEnemyDamaged = delegate { };
    public static event Action OnBossDead = delegate { };
    public static event Action<int, int> OnBossDamaged = delegate { };

    [SerializeField] int maxHealth = 100;
    int _currentHealth = 100;
    EnemyAI enemyAI;

    private void OnEnable()
    {
        _currentHealth = maxHealth;
    }

    private void Start()
    {
        enemyAI = GetComponent<EnemyAI>();   
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        if (enemyAI.enemyType is EnemyAI.EnemyType.Normal)
            OnEnemyDamaged?.Invoke(_currentHealth, maxHealth);
        
        else
            OnBossDamaged?.Invoke(_currentHealth, maxHealth);

        if (_currentHealth <= 0)
        {
            if (enemyAI.enemyType is EnemyAI.EnemyType.Normal)
                OnEnemyDead?.Invoke();
            
            else
                OnBossDead?.Invoke();

            Destroy(gameObject);
            _currentHealth = maxHealth;
        }
    }

    public int GetCurrentHealth() => _currentHealth;

    public int GetMaxHealth() => maxHealth;
}