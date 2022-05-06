using System.Collections;
using UnityEngine;

namespace SheriffsInTown
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(SpecialSkill))]
    [RequireComponent(typeof(PlayerShooting))]
    public class PlayerStunEffect : MonoBehaviour
    {
        [SerializeField] CharacterController playerCharacterController;
        [SerializeField] SpecialSkill playerSpecialSkill;
        [SerializeField] PlayerMovement playerMovement;
        [SerializeField] PlayerShooting playerShooting;
        [SerializeField] Animator stunEffect;

        private void Start()
        {
            stunEffect.gameObject.SetActive(false);
            BossAttackModule.OnBossLanded += StunPlayer;
        }

        private void OnDestroy()
        {
            BossAttackModule.OnBossLanded -= StunPlayer;
        }

        private void StunPlayer(int damage, float stunTime)
        {
            if (playerCharacterController.isGrounded)
                StartCoroutine(StunPlayer(stunTime));
        }

        IEnumerator StunPlayer(float stunTime)
        {
            stunEffect.gameObject.SetActive(true);
            stunEffect.SetBool("PlayStunEffect", true);
            playerSpecialSkill.enabled = false;
            playerMovement.enabled = false;
            playerShooting.enabled = false;

            yield return new WaitForSeconds(stunTime);
            stunEffect.gameObject.SetActive(false);
            stunEffect.SetBool("PlayStunEffect", false);
            playerSpecialSkill.enabled = true;
            playerMovement.enabled = true;
            playerShooting.enabled = true;
        }
    }
}