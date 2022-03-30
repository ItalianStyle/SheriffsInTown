using System;
using UnityEngine;

public class EnemyHealthSystem : MonoBehaviour
{
    public event Action OnEnemyDead = delegate { };
    public event Action<int, int> OnEnemyDamaged = delegate { };

    [SerializeField] int maxHealth = 100;
    [SerializeField] int _currentHealth = 100;

    private void OnEnable()
    {
        //TriggerTrap.OnPlayerTrap += (damage) => SetCurrentHealth(damage);
        _currentHealth = maxHealth;
    }

    private void OnDisable()
    {
        //TriggerTrap.OnPlayerTrap -= (damage) => SetCurrentHealth(damage);
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        OnEnemyDamaged?.Invoke(_currentHealth, maxHealth);        

        if (_currentHealth <= 0)
        {
            //RespawnSystem.instance.ReloadScene();
            OnEnemyDead?.Invoke();
            Destroy(gameObject);

            _currentHealth = maxHealth;
        }
    }

    public int GetCurrentHealth() => _currentHealth;

    public int GetMaxHealth() => maxHealth;
}