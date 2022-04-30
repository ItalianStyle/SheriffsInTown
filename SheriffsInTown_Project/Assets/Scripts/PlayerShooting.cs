using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;

public class PlayerShooting : MonoBehaviour
{
    public static event Action<float> OnPlayerStartReloading = delegate { };
    public static event Action<bool, int> OnPlayerFinishedReloading = delegate { };
    public static event Action<bool> OnPlayerChangedFireMode = delegate { };
    public static event Action<bool, float, float> OnShotFired = delegate { };

    //[Tooltip("Quanto e' largo il cerchio di accuratezza")]
    //[SerializeField] float spreadLimit = 2.0f;

    [Header("Statistiche attuali")]
    [Tooltip("Rateo di fuoco (proiettili al secondo)")]
    [SerializeField] float rateOfFire = 1f;

    [Tooltip("Quali layer puo' colpire il giocatore con il raycast?")]
    [SerializeField] LayerMask _layerMask;

    //Variabile di appoggio per salvare il valore standard del rateo di fuoco quando si attiva l'abilita' speciale
    float rateOfFireReference;

    //Quanti proiettili contiene questa modalita' prima di ricaricare
    int _currentMaxCapacity;

    //Indica la quantita' attuale di munizioni
    int _currentCapacity;

    //Distanza massima che raggiunge lo sparo
    float _currentAttackRange;

    //Danno inflitto
    int _currentDamage;

    //Tempo impiegato per ricaricare tutta la clip di proiettili
    float _currentReloadTime;

    [Header("Riferimenti")]

    [Tooltip("Qui vanno le due modalita' di fuoco del personaggio")]
    [SerializeField] ShootingMode[] shootingModes;
    
    [SerializeField] ParticleSystem hitEffectPrefab = null;

    public int CurrentCapacity
    {
        get => _currentCapacity;

        set
        {
            _currentCapacity = value;
            //Limito il valore appena salvato nel range 0 e massima capacita'
            _currentCapacity = Mathf.Clamp(_currentCapacity, 0, _currentMaxCapacity);

            //Aggiorna la UI relativa alle munizioni se non ha ricaricato
            if (_currentCapacity != _currentMaxCapacity) 
                OnShotFired?.Invoke(CurrentShootingMode.isDoubleGunType, _currentCapacity, _currentMaxCapacity);
                
            else
                OnPlayerFinishedReloading?.Invoke(CurrentShootingMode.isDoubleGunType, _currentMaxCapacity);     
        }
    }

    public float AttackRange => _currentAttackRange;
    public LayerMask LayerMask => _layerMask;

    bool canShoot = true;   //Quando la pistola puo' sparare
    bool isSkillActive = true;

    ShootingMode CurrentShootingMode => shootingModes[_currentShootingModeIndex];
    int _currentShootingModeIndex = 0;

    Camera cam;
    Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
                         //+ new Vector3(Random.insideUnitCircle.x, Random.insideUnitCircle.y, 0) * spreadLimit;

    private void Start()
    {
        cam = Camera.main;

        
        SetShootingMode(isDoubleShootingMode: true);
        CurrentCapacity = _currentMaxCapacity;
        isSkillActive = false;
        rateOfFireReference = rateOfFire;

        SpecialSkill.OnActivatedSkill += BoostShooting;
        SpecialSkill.OnFinishedSkill += ResetShooting;

        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState newGameState)
    {
        enabled = newGameState == GameState.Gameplay;
    }

    void Update()
    {
        if(CurrentCapacity < _currentMaxCapacity && Input.GetKeyDown(KeyCode.R))
        {
            //Ricarica tutta la clip dei proiettili
            StartCoroutine(Reload(isPlayerReloading: true));
            OnPlayerStartReloading?.Invoke(_currentReloadTime);
        }
        else if(Input.GetKeyDown(KeyCode.C))
        {
            //Cambia modalita' di sparo
            SetShootingMode(!CurrentShootingMode.isDoubleGunType);   
        }

        //Se la pistola ha almeno 1 munizione, e' passato il tempo di ricarica ed il giocatore sta premendo il tasto
        if (CurrentCapacity > 0 && canShoot && Input.GetKey(KeyCode.Mouse0))
        {
            Shoot();
            if (!isSkillActive)
            {
                CurrentCapacity--;
                if (CurrentCapacity > 0)
                    StartCoroutine(Reload(isPlayerReloading: false));
            }
            else
                StartCoroutine(Reload(isPlayerReloading: false));
        }
    }

    private void OnDestroy()
    {
        SpecialSkill.OnActivatedSkill -= BoostShooting;
        SpecialSkill.OnFinishedSkill -= ResetShooting;

        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    void BoostShooting(SpecialSkill skill)
    {
        isSkillActive = true;
        // Rate of fire final = rate of fire - % 
        rateOfFire -= rateOfFire * skill.RateOfFire_Bonus;
        if (rateOfFire < .1f)
            rateOfFire = .1f;
    }

    void ResetShooting()
    {
        isSkillActive = false;
        rateOfFire = rateOfFireReference;
    }

    void SetShootingMode(bool isDoubleShootingMode)
    {
        //Seleziono la modalita di sparo
        _currentShootingModeIndex = isDoubleShootingMode ? 0 : 1;

        //Aggiorno le statistiche del componente secondo la nuova modalita' di sparo
        _currentAttackRange = CurrentShootingMode.attackRange;
        _currentDamage = CurrentShootingMode.damage;
        _currentReloadTime = CurrentShootingMode.reloadTime;
        _currentMaxCapacity = CurrentShootingMode.maxCapacity;
        
        //Mi assicuro di limitare la capacita' della singola pistola quando si passa da modalita' doppia a singola
        CurrentCapacity = Mathf.Clamp(CurrentCapacity, 0, _currentMaxCapacity);

        OnPlayerChangedFireMode?.Invoke(CurrentShootingMode.isDoubleGunType);
    }

    IEnumerator Reload(bool isPlayerReloading)
    {
        canShoot = false;
        float timeToWait = isPlayerReloading ? _currentReloadTime : rateOfFire;
        for (float timer = 0; timer < timeToWait; timer += Time.deltaTime)
        {
            UI_Manager.instance.SetReloadBarFillAmount(CurrentShootingMode.isDoubleGunType, timer, timeToWait);
            yield return null;
        }

        if (isPlayerReloading)
            CurrentCapacity = CurrentShootingMode.maxCapacity;
        
        canShoot = true;
    }

    void Shoot()
    {
        //http://codesaying.com/understanding-screen-point-world-point-and-viewport-point-in-unity3d/
        Ray ray = cam.ScreenPointToRay(screenCenter);
        
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, _currentAttackRange, _layerMask))
        {
            //Debug.Log($"Colpito --> {hit.transform.name}");
            //Debug.DrawRay(ray.origin, ray.direction.normalized * (ray.origin - hit.point).magnitude, Color.red, 2f);

            //Crea l'effetto
            ParticleSystem effect = Instantiate(hitEffectPrefab);

            //Posiziona l'effetto sul punto di impatto
            effect.transform.position = hit.point;

            //Distruggi l'effetto quando finisce l'effetto
            Destroy(effect.gameObject, effect.main.duration);


            if (hit.collider.TryGetComponent(out EnemyHealthSystem healthSys))
                healthSys.TakeDamage(_currentDamage);

            else if (hit.collider.TryGetComponent(out Barrel barrel))
            {
                Debug.Log("Colpito barile");
                barrel.Destroy();
            }
        }
    }
}