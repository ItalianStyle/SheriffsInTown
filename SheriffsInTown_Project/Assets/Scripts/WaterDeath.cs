using UnityEngine;

namespace SheriffsInTown
{
    public class WaterDeath : MonoBehaviour
    {
        private void OnTriggerExit(Collider other)
        {
            //Se il player è uscito da sotto il trigger dell'acqua
            if (other.CompareTag("Player") && other.transform.position.y < transform.position.y)
            {
                //E' annegato, il giocatore muore e perde 1 vita
                other.GetComponent<PlayerHealthSystem>().CurrentHealth -= 999;
            }
        }
    }
}