using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletImpact : MonoBehaviour
{
    public float Damage;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Enemy"))
        {
            collision.transform.GetComponent<PlayerHealthSystem>().SetCurrentHealth(20,true);
        }
        Destroy(gameObject);
    }
}
