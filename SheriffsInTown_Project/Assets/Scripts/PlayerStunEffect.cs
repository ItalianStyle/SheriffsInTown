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

        private void Start()
        {
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
            playerSpecialSkill.enabled = false;
            playerMovement.enabled = false;
            playerShooting.enabled = false;

            yield return new WaitForSeconds(stunTime);
            playerSpecialSkill.enabled = true;
            playerMovement.enabled = true;
            playerShooting.enabled = true;
        }
    }
}