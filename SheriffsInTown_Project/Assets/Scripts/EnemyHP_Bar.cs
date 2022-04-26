using UnityEngine;
using UnityEngine.UI;

public class EnemyHP_Bar : MonoBehaviour
{
    Transform targetToLook; //Riferimento per puntare il canvas verso il giocatore
    EnemyHealthSystem enemyHealth;  //Riferimento per ascoltare l'evento di subimento danni

    [Tooltip("Riferimento alla barra vita del nemico")]
    [SerializeField] Image hpBar;

    private void Awake()
    {
        //Prendo i riferimenti necessari
        targetToLook = Camera.main.transform;
        enemyHealth = GetComponentInParent<EnemyHealthSystem>();
    }

    void Start()
    {
        //Quando il nemico prende danno aggiorna la barra vita del nemico
        enemyHealth.OnEnemyDamaged += UpdateHP_Bar;
    }

    private void UpdateHP_Bar(int currentHealth, int maxHealth)
    {
        //Stabilisco la quantita' di riempimento della barra vita
        hpBar.fillAmount = currentHealth / (float)maxHealth;
    }

    void Update()
    {
        //Punta il canvas verso il player
        transform.LookAt(targetToLook);
    }

    private void OnDestroy()
    {
        //Quando muore il nemico non ascoltare piu' l'evento
        enemyHealth.OnEnemyDamaged -= UpdateHP_Bar;
    }
}