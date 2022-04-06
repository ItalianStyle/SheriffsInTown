using UnityEngine;

public class BulletImpact : MonoBehaviour
{
    public float Damage;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Enemy"))
        {
            collision.transform.GetComponent<PlayerHealthSystem>().CurrentHealth -= 20;
        }
        Destroy(gameObject);
    }
}
