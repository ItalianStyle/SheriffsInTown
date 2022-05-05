using System.Collections;
using UnityEngine;

namespace SheriffsInTown
{
    [RequireComponent(typeof(Rigidbody))]
    public class Bullet : MonoBehaviour
    {
        int damage; //Danno da infliggere al giocatore se il proiettile lo colpisce
        Rigidbody bulletRigidbody;  //Riferimento al rigibody per "spararlo"

        private void Awake()
        {
            bulletRigidbody = GetComponent<Rigidbody>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.CompareTag("Player"))
            {
                //Infliggi danni al giocatore
                other.transform.GetComponent<PlayerHealthSystem>().CurrentHealth -= damage;
            }
            //In ogni caso disabilita il proiettile
            gameObject.SetActive(false);
        }

        public void Fire(float force, int newDamage, float lifeTime)
        {
            damage = newDamage;     //Passa il danno da BossAttackModule al proiettile

            bulletRigidbody.velocity = Vector3.zero;    //Resetta l'eventuale velocità accumulata precedente
            bulletRigidbody.AddForce(transform.forward * force);    //Spara il proiettile

            StartCoroutine(DisableAfter(lifeTime));     //Disattiva il proiettile dopo un periodo di tempo
        }

        IEnumerator DisableAfter(float time)
        {
            yield return new WaitForSeconds(time);
            gameObject.SetActive(false);
        }
    }
}