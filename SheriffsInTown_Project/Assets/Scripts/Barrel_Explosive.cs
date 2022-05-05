using UnityEngine;

namespace SheriffsInTown
{
    public class Barrel_Explosive : Barrel
    {
        [Tooltip("Danni che infligge l'esplosione")]
        [SerializeField] int damage;

        [Tooltip("Raggio dell'esplosione.\nDeve corrispondere ASSOLUTAMENTE al raggio dello SphereCollider per rendersi conto visivamente del cambiamento")]
        [SerializeField] float explosionRadius;

        [Tooltip("Quali oggetti può colpire l'esplosione?")]
        [SerializeField] LayerMask _layerMask;

        //Chiamato dal PlayerShooting quando colpisce il barile
        public override void Destroy()
        {
            base.Destroy();

            //Danneggia chiunque sia entro il raggio
            //https://docs.unity3d.com/ScriptReference/Physics.OverlapSphere.html
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, _layerMask);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent(out PlayerHealthSystem player))
                {
                    player.CurrentHealth -= damage;
                }
                else if (hitCollider.TryGetComponent(out EnemyHealthSystem enemy))
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
    }
}