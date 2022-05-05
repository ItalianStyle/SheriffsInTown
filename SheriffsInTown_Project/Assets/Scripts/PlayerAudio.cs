using System.Collections.Generic;
using UnityEngine;

namespace SheriffsInTown
{
    public class PlayerAudio : MonoBehaviour
    {
        [Tooltip("Lista di effetti audio quando il giocatore viene colpito")]
        [SerializeField] List<AudioClip> playerHitsSFX;
        [SerializeField] AudioClip singleGunShootSFX;
        [SerializeField] AudioClip doubleGunShootSFX;

        #region Riferimenti
        AudioSource playerAudioSource;
        AudioSource gunDX_AudioSource;
        AudioSource gunSX_AudioSource;
        #endregion Riferimenti

        private void Awake()
        {
            playerAudioSource = GetComponent<AudioSource>();
            gunDX_AudioSource = transform.Find("Mesh_Corpo/PistolaDX").GetComponent<AudioSource>();
            gunSX_AudioSource = transform.Find("Mesh_Corpo/PistolaSX").GetComponent<AudioSource>();
        }

        private void Start()
        {
            PlayerHealthSystem.OnPlayerDamaged += PlayHitSFX;
            PlayerShooting.OnGunShotFire += PlayShotClip;
        }

        private void OnDestroy()
        {
            PlayerHealthSystem.OnPlayerDamaged -= PlayHitSFX;
            PlayerShooting.OnGunShotFire -= PlayShotClip;
        }

        private void PlayShotClip(bool isDoubleGunShootingMode, bool isRightGunShot)
        {
            if (isDoubleGunShootingMode)
            {
                if (isRightGunShot)
                    gunDX_AudioSource.PlayOneShot(doubleGunShootSFX);
                else
                    gunSX_AudioSource.PlayOneShot(doubleGunShootSFX);
            }
            else
                gunDX_AudioSource.PlayOneShot(singleGunShootSFX);

        }

        private void PlayHitSFX(int currentHealth, int maxHealth)
        {
            AudioClip hitSFX;
            if (playerHitsSFX.Count > 0)
            {
                hitSFX = playerHitsSFX[Random.Range(0, playerHitsSFX.Count)];
                playerAudioSource.PlayOneShot(hitSFX);
            }
        }
    }
}