using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SheriffsInTown
{
    public class PlayerAimReticle : MonoBehaviour
    {
        [Tooltip("Mirino da utilizzare per farlo apparire quando il giocatore mira ad un punto entro il raggio")]
        [SerializeField] Image reticle;

        Camera cam;
        PlayerShooting playerShooting;  //Serve per prendere i valori di range e layermask

        // Start is called before the first frame update
        void Start()
        {
            //Prendo i riferimenti necessari
            cam = Camera.main;
            playerShooting = GetComponent<PlayerShooting>();

            //reticle.enabled = false;
            StartCoroutine(CheckReticle());
        }

        IEnumerator CheckReticle()
        {
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            while (true)
            {
                Ray ray = cam.ScreenPointToRay(screenCenter);
                reticle.enabled = Physics.Raycast(ray, out RaycastHit info, playerShooting.AttackRange, playerShooting.LayerMask);
                
                if(reticle.enabled)
                {
                    reticle.color = info.collider.CompareTag("Enemy") ? Color.red : Color.white;
                }
                yield return new WaitForSeconds(.1f);
            }
        }
    }
}