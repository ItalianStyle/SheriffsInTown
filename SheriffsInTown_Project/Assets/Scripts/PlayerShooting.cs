using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;

public class PlayerShooting : MonoBehaviour
{
    public static event Action OnPlayerStartReloading = delegate { };
    public static event Action OnPlayerFinishedReloading = delegate { };

    [Tooltip("Qui vanno le due modalita' di fuoco del personaggio")]
    [SerializeField] ShootingMode[] shootingModes;
    
    //public static event Action<bool> OnShotFired = delegate { };

    //[Tooltip("Quanto e' largo il cerchio di accuratezza")]
    //[SerializeField] float spreadLimit = 2.0f;
    
    [Header("Statistiche attuali")]
    [Tooltip("Rateo di fuoco (proiettili al secondo)")]
    [SerializeField] float rateOfFire = 1f;
    //Quanti proiettili contiene questa modalita' prima di ricaricare
    int _maxCapacity;
    int _currentCapacity;

    //Distanza massima che raggiunge lo sparo
    float _attackRange;

    //Danno inflitto
    int _damage;

    //Tempo impiegato per ricaricare tutta la clip di proiettili
    float _reloadTime;

    [SerializeField] TMP_Text munitionsText;

    [SerializeField] ParticleSystem hitEffectPrefab = null;

    [Tooltip("Quali layer puo' colpire il giocatore con il raycast?")]
    [SerializeField] LayerMask _layerMask;

    public int CurrentCapacity
    {
        get => _currentCapacity;

        set
        {
            _currentCapacity = value;
            _currentCapacity = Mathf.Clamp(_currentCapacity, 0, _maxCapacity);

            UpdateMunitionsText();
        }
    }

    public float AttackRange => _attackRange;
    public LayerMask LayerMask => _layerMask;

    bool canShoot = true;   //Quando la pistola può sparare
    bool canListenReloadInput = true;

    ShootingMode CurrentShootingMode => shootingModes[_currentShootingModeIndex];
    int _currentShootingModeIndex = 0;

    Camera cam;
    Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
                         //+ new Vector3(Random.insideUnitCircle.x, Random.insideUnitCircle.y, 0) * spreadLimit;

    private void Start()
    {
        cam = Camera.main;

        SetShootingMode(isDoubleShootingMode: true);
        CurrentCapacity = _maxCapacity;
    }

    void Update()
    {
        
        if(CurrentCapacity < _maxCapacity && Input.GetKeyDown(KeyCode.R))
        {
            //Ricarica tutta la clip dei proiettili
            StartCoroutine(Reload(isPlayerReloading: true));
            OnPlayerStartReloading?.Invoke();
        }
        else if(Input.GetKeyDown(KeyCode.C))
        {
            //Cambia modalita' di sparo
            SetShootingMode(!CurrentShootingMode.isDoubleGunType);
        }

        if (canShoot && Input.GetKey(KeyCode.Mouse0))
        {
            Shoot();
            CurrentCapacity--;
            if (CurrentCapacity > 0)
            {
                StartCoroutine(Reload(isPlayerReloading: false));
            }
            else
            {
                CurrentCapacity = 0;
            }
        }
    }

    void SetShootingMode(bool isDoubleShootingMode)
    {
        _currentShootingModeIndex = isDoubleShootingMode ? 0 : 1;

        _maxCapacity = CurrentShootingMode.maxCapacity;
        //Mi assicuro di limitare la capacita' della singola pistola quando si passa da modalita' doppia a singola
        CurrentCapacity = Mathf.Clamp(CurrentCapacity, 0, _maxCapacity);
        
        _attackRange = CurrentShootingMode.attackRange;
        _damage = CurrentShootingMode.damage;
        _reloadTime = CurrentShootingMode.reloadTime;
    }

    private void UpdateMunitionsText()
    {
        munitionsText.text = $"{CurrentCapacity} / {_maxCapacity}";
    }

    IEnumerator Reload(bool isPlayerReloading)
    {
        canShoot = false;
        
        yield return new WaitForSeconds(isPlayerReloading ? _reloadTime : rateOfFire);

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

        if (Physics.Raycast(ray, out hit, _attackRange, _layerMask))
        {
            //http://codesaying.com/understanding-screen-point-world-point-and-viewport-point-in-unity3d/
            if (hit.collider.TryGetComponent(out EnemyHealthSystem healthSys))
            {
                ParticleSystem effect = Instantiate(hitEffectPrefab);

                effect.transform.position = hit.point;
                effect.Play();
                healthSys.TakeDamage(_damage);
            }
            //OnShotFired?.Invoke(CompareTag("Player"));
        }
    }
}