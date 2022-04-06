using System;
using UnityEngine;

public class PlayerHealthSystem : MonoBehaviour
{
    public static event Action<GameObject> OnPlayerDead = delegate { };
    public static event Action<int, int> OnPlayerDamaged = delegate { };
    public static event Action<int, int> OnPlayerHealed = delegate { };
    public static event Action<int, int> OnPlayerHealthChanged = delegate { };

    [SerializeField] int maxHealth = 100;
    [SerializeField] int _currentHealth = 100;

    public int CurrentHealth
    {
        get => _currentHealth;
        set
        {
            int oldHP = _currentHealth;
            _currentHealth = value;

            if (oldHP > _currentHealth)
                OnPlayerDamaged?.Invoke(_currentHealth, maxHealth);
            
            else
                 OnPlayerHealed?.Invoke(_currentHealth, maxHealth);

            if (_currentHealth <= 0)
            {
                //RespawnSystem.instance.ReloadScene();
                OnPlayerDead?.Invoke(gameObject);

                _currentHealth = maxHealth;
            }

            OnPlayerHealthChanged?.Invoke(_currentHealth, maxHealth);
        }
    }
    public static PlayerHealthSystem instance;

    private void OnEnable()
    {
        instance = this;

        //TriggerTrap.OnPlayerTrap += (damage) => SetCurrentHealth(damage);
        CurrentHealth = maxHealth;
    }

    private void OnDisable()
    {
        //TriggerTrap.OnPlayerTrap -= (damage) => SetCurrentHealth(damage);
    }
}