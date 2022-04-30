using System;
using UnityEngine;

// Makes objects float up & down while gently spinning.
[RequireComponent(typeof(Collider))]
public class Pickup : MonoBehaviour
{
    public static event Action<float> OnStarPickupTaken = delegate { };
    public static event Action<int> OnHealPickupTaken = delegate { };
    public static event Action<float, float> OnHatPickupTaken = delegate { };
    public static event Action OnGoldGunPickupTaken = delegate { };

    enum Direction { X, Y, Z }
    public enum PickupType { NotDefined, Hat, GoldGun, Star, Heal }

    #region Variables
    // User Inputs
    public PickupType pickupType = PickupType.NotDefined;

    [Header("Floating stats")]
    [SerializeField] float amplitude = 0.5f;
    [SerializeField] float frequency = 1f;

    [Header("Rotation stats")]
    [SerializeField] Direction rotationDirection = Direction.Y;
    [SerializeField] float rotatingSpeed = 0f;

    [Header("PowerUp properties")]
    [Tooltip("Quantità da recuperare per la barra dell'abilità speciale quando il player raccoglie la stella")]
    [SerializeField] [Range(0f,100f)] float specialSkillBarAmount = 20f;

    [Tooltip("Quantità di HP da recuperare quando il giocatore raccoglie la boccetta")]
    [SerializeField] [Min(0f)] int healthToRecover = 10;

    [Tooltip("La nuova velocità di base del player quando raccoglie il cappello")]
    [SerializeField] float newMovementSpeed;

    [Tooltip("La nuova velocità di corsa del player quando raccoglie il cappello")]
    [SerializeField] float newRunMovementSpeed;


    // Position Storage Variables
    Vector3 posOffset = new Vector3();
    Vector3 tempPos = new Vector3();
    #endregion

    #region Unity Methods
    void Start()
    {
        // Store the starting position & rotation of the object
        posOffset = transform.position;
    }

    void Update()
    {
        // Float up/down with a Sin()
        tempPos = posOffset;
        tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;

        transform.position = tempPos;

        switch (rotationDirection)
        {
            case Direction.X:
                transform.Rotate(Vector3.right * rotatingSpeed * Time.deltaTime);
                break;

            case Direction.Y:
                transform.Rotate(Vector3.up * rotatingSpeed * Time.deltaTime);
                break;

            case Direction.Z:
                transform.Rotate(Vector3.forward * rotatingSpeed * Time.deltaTime);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch (pickupType)
            {
                case PickupType.Star:
                    //Il giocatore raccoglie la stella solo quando la barra non è piena e non sta utilizzando l'abilità
                    if (!other.GetComponent<SpecialSkill>().canActivateSkill)
                    {
                        OnStarPickupTaken?.Invoke(specialSkillBarAmount);
                        gameObject.SetActive(false);
                    }
                    break;

                case PickupType.Heal:
                    //Il giocatore raccoglie la cura solo se non è full vita
                    if(!other.GetComponent<PlayerHealthSystem>().IsMaxHealth)
                    {
                        OnHealPickupTaken?.Invoke(healthToRecover);
                        gameObject.SetActive(false);
                    }
                    break;

                case PickupType.Hat:
                    //Il giocatore raccoglie il cappello
                    OnHatPickupTaken?.Invoke(newMovementSpeed, newRunMovementSpeed);
                    gameObject.SetActive(false);
                    break;

                case PickupType.GoldGun:
                    //Il giocatore raccoglie la pistola dorata
                    OnGoldGunPickupTaken?.Invoke();
                    gameObject.SetActive(false);
                    break;
            }           
        }
    }
    #endregion
}