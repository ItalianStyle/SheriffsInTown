using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpecialSkill : MonoBehaviour
{
    public static event Action<SpecialSkill> OnActivatedSkill = delegate { };
    public static event Action OnFinishedSkill = delegate { };

    //Quando questo numero raggiunge 100 indica che l'abilita' e' pronta ad essere utilizzata
    int _currentSkillBar;
    int maxSkillBar = 100;

    [SerializeField] Image specialSkillImage;

    [Tooltip("Nuova velocita' di movimento del giocatore")]
    [SerializeField] float _newMovementSpeed;

    [Tooltip("% di bonus da dare al rateo di fuoco\nConversione: 1 -> 100%")]
    [SerializeField] float _bonusRateOfFire = 1f;

    [Tooltip("Durata dell'effetto")]
    [SerializeField] float _duration;

    bool _canActivateSkill;

    bool canActivateSkill
    {
        get => _canActivateSkill;

        set
        {
            Color imageColor = specialSkillImage.color;
            imageColor.a = value ? 1f : .5f;
            specialSkillImage.color = imageColor;

            _canActivateSkill = value;
        }
    }
    
    int CurrentSkillBar
    {
        get => _currentSkillBar;

        set
        {
            _currentSkillBar = Mathf.Clamp(value, 0, 100);
            specialSkillImage.color = _currentSkillBar < 100 ? Color.gray : Color.green;
            canActivateSkill = _currentSkillBar == maxSkillBar;
        }
    }
    public float NewMovementSpeed => _newMovementSpeed;
    public float RateOfFire_Bonus => _bonusRateOfFire;

    private void Start()
    {
        CurrentSkillBar = maxSkillBar;
        PlayerShooting.OnPlayerStartReloading += () => canActivateSkill = false;
        PlayerShooting.OnPlayerFinishedReloading += () => canActivateSkill = true;
    }

    private void Update()
    {
        if(canActivateSkill && Input.GetKeyDown(KeyCode.Tab) && CurrentSkillBar >= maxSkillBar)
        {
            CurrentSkillBar = 0;
            OnActivatedSkill?.Invoke(this);
            StartCoroutine(SkillCountdown());
        }
    }

    private void OnDestroy()
    {
        PlayerShooting.OnPlayerStartReloading -= () => canActivateSkill = false;
        PlayerShooting.OnPlayerFinishedReloading -= () => canActivateSkill = true;
    }

    private IEnumerator SkillCountdown()
    {
        specialSkillImage.color = Color.yellow;
        yield return new WaitForSeconds(_duration);
        OnFinishedSkill?.Invoke();
        CurrentSkillBar = 0;
        StartCoroutine(SkillReload());
    }

    IEnumerator SkillReload()
    {
        yield return new WaitForSeconds(1f);
        CurrentSkillBar = maxSkillBar;
    }
}
