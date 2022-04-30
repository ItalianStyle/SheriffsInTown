using System;
using UnityEngine;

public class PlayerHealthSystem : MonoBehaviour
{
    public static event Action<int> OnLivesChanged = delegate { };
    public static event Action<GameObject> OnPlayerDead = delegate { };
    public static event Action<int, int> OnPlayerDamaged = delegate { };
    public static event Action<int, int> OnPlayerHealed = delegate { };
    public static event Action<int, int> OnPlayerHealthChanged = delegate { };

    [SerializeField] int _maxHealth = 100;
    [SerializeField] int _currentHealth = 100;

    [SerializeField] int _totalLives = 3;

    //Vite a disposizione del giocatore prima di perdere la partita
    int TotalLives
    {
        get => _totalLives;

        set
        {
            _totalLives = value;
            OnLivesChanged?.Invoke(_totalLives);
        }
    }

    //Proprietà per gestire la salute del giocatore
    public int CurrentHealth
    {
        get => _currentHealth;
        set
        {
            //Salvo la salute corrente del giocatore in una variabile dummy
            int oldHP = _currentHealth;
            _currentHealth = value;

            //Invoco l'evento relativo se è stato ferito o curato
            if (oldHP > _currentHealth)
                OnPlayerDamaged?.Invoke(_currentHealth, _maxHealth);
            else
                 OnPlayerHealed?.Invoke(_currentHealth, _maxHealth);

            if (_currentHealth <= 0)
            {
                OnPlayerDead?.Invoke(gameObject);

                TotalLives--;
                if (TotalLives <= 0)
                {
                    //Gioco perso
                    GameStateManager.Instance.SetState(GameState.Lost);
                }
                else
                {
                    //Respawna
                    RespawnManager.Instance.RespawnPlayer(gameObject);
                }             

                _currentHealth = _maxHealth;
            }

            OnPlayerHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
    }
    public bool IsMaxHealth => _currentHealth == _maxHealth;

    public static PlayerHealthSystem instance;

    private void OnEnable()
    {
        instance = this;
        CurrentHealth = _maxHealth;
    }

    private void Start()
    {
        Pickup.OnHealPickupTaken += HandlePickup;
    }

    private void OnDestroy()
    {
        Pickup.OnHealPickupTaken -= HandlePickup;
    }

    private void HandlePickup(int healthToRecover)
    {
        CurrentHealth += healthToRecover;
    }
}