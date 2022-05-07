using System.Collections;
using UnityEngine;

public class ShootFX_Manager : MonoBehaviour
{
    static ShootFX_Manager instance;

    private void Awake()
    {
        instance = this;
    }

    public static void PlayMuzzleFlashFX(Transform spawnPointTransform)
    {
        ParticleSystem particles = ObjectPooler.SharedInstance.GetPooledObject("MuzzleFlashFX").GetComponent<ParticleSystem>();

        particles.transform.parent = spawnPointTransform;
        particles.transform.localPosition = Vector3.zero;
        particles.transform.localRotation = Quaternion.Euler(0, -180f, 0f);

        PlayParticles(particles);
    }

    public static void PlayBulletHitFX(Vector3 hitPoint)
    {
        ParticleSystem particles = ObjectPooler.SharedInstance.GetPooledObject("BulletHitFX").GetComponent<ParticleSystem>();

        //Posiziona il particellare sulla bocca di fuoco destra o sinistra in base al bool
        particles.transform.position = hitPoint;

        PlayParticles(particles);
    }

    public static void PlayBigExplosionFX(Vector3 spawnPoint)
    {
        ParticleSystem particles = ObjectPooler.SharedInstance.GetPooledObject("BigExplosionFX").GetComponent<ParticleSystem>();
        
        //Posiziona il particellare sulla bocca di fuoco destra o sinistra in base al bool
        particles.transform.position = spawnPoint;

        PlayParticles(particles);
    }

    IEnumerator DisableParticles(GameObject particleSystem, float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        particleSystem.transform.parent = null;
        particleSystem.SetActive(false);
    }

    static void PlayParticles(ParticleSystem particles)
    {
        particles.gameObject.SetActive(true);
        particles.Play();
        instance.StartCoroutine(instance.DisableParticles(particles.gameObject, particles.main.duration));    //Disabilita l'effetto quando finisce
    }
}