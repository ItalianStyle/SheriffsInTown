using System;
using UnityEngine;

public class PlayerHealthSystem : MonoBehaviour
{
    public static event Action OnPlayerDead = delegate { };
    public static event Action<int, int> OnPlayerDamaged = delegate { };
    public static event Action<int, int> OnPlayerHealed = delegate { };

    [SerializeField] int maxHealth = 100;
    [SerializeField] int _currentHealth = 100;

    public static PlayerHealthSystem instance;

    private void OnEnable()
    {
        instance = this;

        //TriggerTrap.OnPlayerTrap += (damage) => SetCurrentHealth(damage);
        _currentHealth = maxHealth;
    }

    private void OnDisable()
    {
        //TriggerTrap.OnPlayerTrap -= (damage) => SetCurrentHealth(damage);
    }

    public void SetCurrentHealth(int amount, bool isDamage)
    {
        amount = Mathf.Abs(amount);
        if (isDamage)
        {
            _currentHealth -= amount;
            OnPlayerDamaged?.Invoke(_currentHealth, maxHealth);
        }
        else
        {
            _currentHealth += amount;
            OnPlayerHealed?.Invoke(_currentHealth, maxHealth);
        }
        

        if (_currentHealth <= 0)
        {
            //RespawnSystem.instance.ReloadScene();
            OnPlayerDead?.Invoke();
            gameObject.SetActive(false);

            _currentHealth = maxHealth;
        }
    }

    public int GetCurrentHealth() => _currentHealth;

    public int GetMaxHealth() => maxHealth;
}