using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerShooting : MonoBehaviour
{
    //public static event Action<bool> OnShotFired = delegate { };

    //[Tooltip("Quanto e' largo il cerchio di accuratezza")]
    //[SerializeField] float spreadLimit = 2.0f;
    
    [Tooltip("Distanza massima che raggiunge lo sparo")]
    [SerializeField] float _attackRange;
    [Tooltip("Danno inflitto")]
    [SerializeField] int damage;

    [Tooltip("Tempo di ricarica")]
    [SerializeField] float reloadTime;

    [SerializeField] ParticleSystem hitEffectPrefab = null;

    [Tooltip("Quali layer puo' colpire il giocatore con il raycast?")]
    [SerializeField] LayerMask _layerMask;

    public float AttackRange => _attackRange;
    public LayerMask LayerMask => _layerMask;

    bool canShoot = true;   //Quando il giocatore e' pronto a sparare

    Camera cam;
    Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
                         //+ new Vector3(Random.insideUnitCircle.x, Random.insideUnitCircle.y, 0) * spreadLimit;

    private void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (canShoot && Input.GetKey(KeyCode.Mouse0))
        {
            Shoot();
            StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        canShoot = false;
        yield return new WaitForSeconds(reloadTime);
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
                healthSys.TakeDamage(damage);
            }
            //OnShotFired?.Invoke(CompareTag("Player"));
        }
    }
}