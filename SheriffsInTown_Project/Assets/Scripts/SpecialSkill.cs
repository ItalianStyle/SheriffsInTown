using System;
using System.Collections;
using UnityEngine;

public class SpecialSkill : MonoBehaviour
{
    public static event Action<SpecialSkill> OnActivatedSkill = delegate { };
    public static event Action OnFinishedSkill = delegate { };
    public static event Action<float, float> OnSpecialSkillBarChangedValue = delegate { };

    //Quando questo numero raggiunge 100 indica che l'abilita' e' pronta ad essere utilizzata
    float _currentSkillBar;
    float maxSkillBar = 100;

    [Tooltip("Nuova velocita' di movimento del giocatore")]
    [SerializeField] float _newMovementSpeed;

    [Tooltip("% di bonus da dare al rateo di fuoco\nConversione: 1 -> 100%")]
    [SerializeField] float _bonusRateOfFire = 1f;

    [Tooltip("Durata dell'effetto")]
    [SerializeField] float _duration;

    public bool canActivateSkill;
    
    float CurrentSkillBar
    {
        get => _currentSkillBar;

        set
        {
            _currentSkillBar = Mathf.Clamp(value, 0f, 100f);
            canActivateSkill = _currentSkillBar == maxSkillBar;
            OnSpecialSkillBarChangedValue?.Invoke(_currentSkillBar, maxSkillBar);
        }
    }

    public float RateOfFire_Bonus => _bonusRateOfFire;

    private void Start()
    {
        CurrentSkillBar = maxSkillBar;
        PlayerShooting.OnPlayerStartReloading += CantActivateSkill;
        PlayerShooting.OnPlayerFinishedReloading += CanActivateSkill;
        Pickup.OnStarPickupTaken += HandlePickupTaken;
    }

    private void CanActivateSkill(bool isDoubleGunMode, int currentMaxCapacity)
    {
        canActivateSkill = true;
    }

    private void CantActivateSkill(float reloadTime)
    {
        canActivateSkill = false;
    }

    private void HandlePickupTaken(float specialSkillBarAmount)
    {
        CurrentSkillBar += specialSkillBarAmount;
    }

    private void Update()
    {
        if(canActivateSkill && Input.GetKeyDown(KeyCode.Tab) && CurrentSkillBar >= maxSkillBar)
        {
            OnActivatedSkill?.Invoke(this);
            StartCoroutine(SkillCountdown());
        }
    }

    private void OnDestroy()
    {
        PlayerShooting.OnPlayerStartReloading -= CantActivateSkill;
        PlayerShooting.OnPlayerFinishedReloading -= CanActivateSkill;
        Pickup.OnStarPickupTaken -= HandlePickupTaken;
    }

    private IEnumerator SkillCountdown()
    {
        while (CurrentSkillBar > 0)
        {
            CurrentSkillBar -= Time.deltaTime * _duration;
            yield return null;
        }

        OnFinishedSkill?.Invoke();
        CurrentSkillBar = 0;
        //StartCoroutine(SkillReload());
    }

    //Coroutine per quick testing (DA RIMUOVERE)
    IEnumerator SkillReload()
    {
        yield return new WaitForSeconds(1f);
        CurrentSkillBar = maxSkillBar;
    }
}