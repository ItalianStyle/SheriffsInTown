using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;

public class PlayerShooting : MonoBehaviour
{
    public static event Action OnPlayerStartReloading = delegate { };
    public static event Action OnPlayerFinishedReloading = delegate { };

    //public static event Action<bool> OnShotFired = delegate { };

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

    [Tooltip("Inserire qui i modelli delle due pistole")]
    [SerializeField] GameObject[] gunMeshes = new GameObject[2];

    [SerializeField] Image munitionsStateImage;
    [SerializeField] TMP_Text munitionsText;
    [SerializeField] ParticleSystem hitEffectPrefab = null;

    public int CurrentCapacity
    {
        get => _currentCapacity;

        set
        {
            _currentCapacity = value;
            //Limito il valore appena salvato nel range 0 e massima capacita'
            _currentCapacity = Mathf.Clamp(_currentCapacity, 0, _currentMaxCapacity);

            //Aggiorna la UI relativa alle munizioni
            UpdateMunitionsUI();
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

        SpecialSkill.OnActivatedSkill += (skill) => BoostShooting(skill);
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
            OnPlayerStartReloading?.Invoke();
        }
        else if(Input.GetKeyDown(KeyCode.C))
        {
            //Cambia modalita' di sparo
            SetShootingMode(!CurrentShootingMode.isDoubleGunType);
            UpdateMunitionsUI();
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
        SpecialSkill.OnActivatedSkill -= (skill) => BoostShooting(skill);
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

        //Fai apparire la pistola (sulla mano sinistra) se la modalita di sparo e' doppia
        gunMeshes[0].SetActive(isDoubleShootingMode);

        //Aggiorno le statistiche del componente secondo la nuova modalita' di sparo
        _currentAttackRange = CurrentShootingMode.attackRange;
        _currentDamage = CurrentShootingMode.damage;
        _currentReloadTime = CurrentShootingMode.reloadTime;
        _currentMaxCapacity = CurrentShootingMode.maxCapacity;
        
        //Mi assicuro di limitare la capacita' della singola pistola quando si passa da modalita' doppia a singola
        CurrentCapacity = Mathf.Clamp(CurrentCapacity, 0, _currentMaxCapacity);
    }

    private void UpdateMunitionsUI()
    {
        //Aggiorna il riempimento dell'immagine della pistola
        munitionsStateImage.fillAmount = (float) CurrentCapacity / _currentMaxCapacity;
        
        //Aggiorna il testo delle munizioni correnti
        munitionsText.text = $"{CurrentCapacity} / {_currentMaxCapacity}";
    }

    IEnumerator Reload(bool isPlayerReloading)
    {
        canShoot = false;
        for (float timer = 0f; timer <= (isPlayerReloading ? _currentReloadTime : rateOfFire); timer += Time.deltaTime)
        {
            if(isPlayerReloading)
                munitionsStateImage.fillAmount = timer / _currentReloadTime;
            yield return null;
        }

        if (isPlayerReloading)
        {
            CurrentCapacity = CurrentShootingMode.maxCapacity;
            OnPlayerFinishedReloading?.Invoke();
        }
        canShoot = true;
    }

    void Shoot()
    {                            
        Ray ray = cam.ScreenPointToRay(screenCenter);
        
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, _currentAttackRange, _layerMask))
        {
            Debug.Log($"Colpito --> {hit.transform.name}");
            Debug.DrawRay(ray.origin, ray.direction.normalized * (ray.origin - hit.point).magnitude, Color.red, 2f);

            //Crea l'effetto
            ParticleSystem effect = Instantiate(hitEffectPrefab);

            //Posiziona l'effetto sul punto di impatto
            effect.transform.position = hit.point;

            //Distruggi l'effetto quando finisce l'effetto
            Destroy(effect.gameObject, effect.main.duration);

            //http://codesaying.com/understanding-screen-point-world-point-and-viewport-point-in-unity3d/
            if (hit.collider.TryGetComponent(out EnemyHealthSystem healthSys))
            {
                healthSys.TakeDamage(_currentDamage);
            }
            //OnShotFired?.Invoke(CompareTag("Player"));
        }

        else
        {
            Debug.Log("Colpo a vuoto");
        }
    }
}