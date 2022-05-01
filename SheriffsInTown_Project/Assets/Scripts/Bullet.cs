using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [Tooltip("Quanto tempo deve passare prima che sparisca il proiettile")]
    [SerializeField] [Min(0f)] float lifeTime;
    int damage;

    Rigidbody bulletRigidbody;

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        //Quando si attiva il proiettile disattivalo dopo un periodo di tempo
        StartCoroutine(DisableAfter(lifeTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning($"Mi sono triggerato con {other.name}");
        if (other.transform.CompareTag("Player"))
        {
            //Infliggi danni al giocatore
            other.transform.GetComponent<PlayerHealthSystem>().CurrentHealth -= damage;   
        }
        //In ogni caso disabilita il proiettile
        gameObject.SetActive(false);
    }

    public void Fire(float force, int newDamage)
    {
        damage = newDamage;
        bulletRigidbody.AddForce(transform.forward * force);
    }

    IEnumerator DisableAfter(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}
