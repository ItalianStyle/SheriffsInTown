using System;
using System.Collections;
using UnityEngine;

namespace SheriffsInTown
{
    public class PlayerShooting : MonoBehaviour
    {
        #region Eventi
        public static event Action<float> OnPlayerStartReloading = delegate { };
        public static event Action<bool, int> OnPlayerFinishedReloading = delegate { };
        public static event Action<bool> OnPlayerChangedFireMode = delegate { };
        public static event Action<bool, float, float> OnShotFired = delegate { };
        public static event Action<bool, bool> OnGunShotFire = delegate { };
        #endregion Eventi

        #region Variabili per l'inspector
        //[Tooltip("Quanto e' largo il cerchio di accuratezza")]
        //[SerializeField] float spreadLimit = 2.0f;

        [Header("Statistiche attuali")]
        [Tooltip("Rateo di fuoco per ogni pistola (proiettili al secondo)")]
        [SerializeField] float rateOfFire = 1f;

        [Tooltip("Quali layer puo' colpire il giocatore con il raycast?")]
        [SerializeField] LayerMask _layerMask;

        [Header("Riferimenti")]

        [Tooltip("Qui vanno le due modalita' di fuoco delle pistole normali del personaggio\n0 -> modalità doppia\n1 -> modalità singola")]
        [SerializeField] ShootingMode[] normalShootingModes;

        [Tooltip("Qui vanno le due modalita' di fuoco delle pistole dorate del personaggio\n0 -> modalità doppia\n1 -> modalità singola")]
        [SerializeField] ShootingMode[] upgradedShootingModes;

        [Tooltip("Serve a posizionare l'effetto particellare di sparo")]
        [SerializeField] Transform muzzleFlashDXTransform;
        [Tooltip("Serve a posizionare l'effetto particellare di sparo")]
        [SerializeField] Transform muzzleFlashSXTransform;
        #endregion Variabili per l'inspector

        #region Variabili private
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

        bool canShoot = true;   //Quando la pistola puo' sparare
        bool isRightGunShoot = true;    //Definisce su quale pistola posizionare il particellare di sparo
        bool isSkillActive = true;
        bool isNormalGunUsed = true;

        int _currentShootingModeIndex = 0;

        Camera cam;
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        //+ new Vector3(Random.insideUnitCircle.x, Random.insideUnitCircle.y, 0) * spreadLimit;
        #endregion Variabili private

        #region Proprietà
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
        ShootingMode CurrentShootingMode => isNormalGunUsed ? normalShootingModes[_currentShootingModeIndex] : upgradedShootingModes[_currentShootingModeIndex];
        #endregion Proprietà

        #region Metodi Unity

        private void Awake()
        {
            cam = Camera.main;
        }

        private void Start()
        {
            SetShootingMode(isDoubleShootingMode: true);
            CurrentCapacity = _currentMaxCapacity;
            isSkillActive = false;
            isNormalGunUsed = true;
            rateOfFireReference = rateOfFire;

            SpecialSkill.OnActivatedSkill += BoostShooting;
            SpecialSkill.OnFinishedSkill += ResetShooting;

            Pickup.OnGoldGunPickupTaken += SetGoldGunShootingMode;
            GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }



        void Update()
        {
            if (CurrentCapacity < _currentMaxCapacity && Input.GetKeyDown(KeyCode.R))
            {
                //Ferma tutte le eventuali coroutine di ricarica
                StopCoroutine(nameof(Reload));
                //Ricarica tutta la clip dei proiettili
                StartCoroutine(Reload(isPlayerReloading: true));
                OnPlayerStartReloading?.Invoke(_currentReloadTime);
            }
            else if (Input.GetKeyDown(KeyCode.C))
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
            Pickup.OnGoldGunPickupTaken -= SetGoldGunShootingMode;
            GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
        #endregion Metodi Unity

        #region Metodi gestione eventi
        void BoostShooting(SpecialSkill skill)
        {
            //Annullo l'eventuale ricarica in corso
            StopCoroutine(nameof(Reload));
            canShoot = true;

            isSkillActive = true;
            // Rateo di fuoco finale = rateo iniziale - % bonus applicato dalla skill 
            rateOfFire -= rateOfFire * (skill.RateOfFire_Bonus / 100);  //Converto la % in cifra con la virgola
            if (rateOfFire < .1f)
                rateOfFire = .1f;
        }

        void ResetShooting()
        {
            isSkillActive = false;
            rateOfFire = rateOfFireReference;
        }

        private void BlockPlayerShooting(int landingDamage, float stunTime)
        {
            if (GetComponent<CharacterController>().isGrounded)
                StartCoroutine(StunPlayer(stunTime));
        }

        private void SetGoldGunShootingMode()
        {
            isNormalGunUsed = false;
            SetShootingMode(CurrentShootingMode.isDoubleGunType);
        }

        private void OnGameStateChanged(GameState newGameState)
        {
            enabled = newGameState == GameState.Gameplay;
        }
        #endregion Metodi gestione eventi

        #region Metodi personali

        void SetShootingMode(bool isDoubleShootingMode)
        {
            int old = _currentShootingModeIndex;
            //Seleziono la modalita di sparo
            _currentShootingModeIndex = isDoubleShootingMode ? 0 : 1;

            //Aggiorno le statistiche del componente secondo la nuova modalita' di sparo
            _currentAttackRange = CurrentShootingMode.attackRange;
            _currentDamage = CurrentShootingMode.damage;
            _currentReloadTime = CurrentShootingMode.reloadTime;
            _currentMaxCapacity = CurrentShootingMode.maxCapacity;

            //Mi assicuro di limitare la capacita' della singola pistola quando si passa da modalita' doppia a singola
            CurrentCapacity = Mathf.Clamp(CurrentCapacity, 0, _currentMaxCapacity);

            if (old != _currentShootingModeIndex)
                OnPlayerChangedFireMode?.Invoke(CurrentShootingMode.isDoubleGunType);
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

                Transform currentMuzzleFlash;
                if (CurrentShootingMode.isDoubleGunType)
                {
                    currentMuzzleFlash = isRightGunShoot ? muzzleFlashDXTransform : muzzleFlashSXTransform;
                    isRightGunShoot = !isRightGunShoot;
                }
                else
                {
                    currentMuzzleFlash = muzzleFlashDXTransform;
                    isRightGunShoot = false;
                }
                ShootFX_Manager.PlayMuzzleFlashFX(currentMuzzleFlash);
                ShootFX_Manager.PlayBulletHitFX(hit.point);

                //Fai partire l'audio dello sparo
                OnGunShotFire?.Invoke(CurrentShootingMode.isDoubleGunType, isRightGunShoot);

                if (hit.collider.TryGetComponent(out EnemyHealthSystem healthSys))
                {
                    healthSys.TakeDamage(_currentDamage);
                }
                else if (hit.collider.TryGetComponent(out Barrel barrel))
                {
                    barrel.Destroy();
                }
            }
        }

        #endregion Metodi personali

        #region Coroutines
        IEnumerator Reload(bool isPlayerReloading)
        {
            canShoot = false;
            float timeToWait;
            //Se il giocatore sta caricando la cartuccia 
            if (isPlayerReloading)
                timeToWait = _currentReloadTime;
            //Altrimenti verifica in che modalità di sparo siamo
            else
                timeToWait = CurrentShootingMode.isDoubleGunType ? rateOfFire / 2 : rateOfFire;

            for (float timer = 0; timer < timeToWait; timer += Time.deltaTime)
            {
                UI_Manager.instance.SetReloadBarFillAmount(CurrentShootingMode.isDoubleGunType, timer, timeToWait);
                yield return null;
            }

            if (isPlayerReloading)
                CurrentCapacity = CurrentShootingMode.maxCapacity;

            canShoot = true;
        }

        IEnumerator StunPlayer(float stunTime)
        {
            StopCoroutine(nameof(Reload));  //Fermo l'eventuale ricarica in corso
            canShoot = false;   //Il giocatore non può sparare
            yield return new WaitForSeconds(stunTime);
            canShoot = true;    //Il giocatore può tornare a sparare
        }
        #endregion Coroutines
    }
}