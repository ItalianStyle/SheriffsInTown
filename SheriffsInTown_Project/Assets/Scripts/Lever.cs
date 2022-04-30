using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{
    public static event Action OnPlayerNearLever = delegate { };
    public static event Action OnPlayerLeftLever = delegate { };
    public static event Action OnCompletedAction = delegate { };
    public static event Action<float, float> OnCurrentPressedKeyTimeChanged = delegate { };

    [Tooltip("Riferimento al ponte a cui è collegata questa leva")]
    [SerializeField] Gate connectedGate;

    Animator leverAnimator;
    [Tooltip("Tempo richiesto al giocatore di tenere premuto il pulsante")]
    [SerializeField] float maxTimeToPress;
    float currentTimeKeyPressed;        //Contatore per tenere conto del tempo passato dal giocatore a tenere premuto il pulsante

    bool canListenInput;     //Booleana per verificare se il giocatore è nelle vicinanze della leva
    bool isGateClosed;      //Booleana che indica lo stato del ponte

    private void Awake()
    {
        leverAnimator = transform.Find("Lever").GetComponent<Animator>();
    }

    private void Start()
    {
        currentTimeKeyPressed = 0;

        isGateClosed = true;    //Il ponte è chiuso all'inizio del gioco
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isGateClosed)     //Se il player si avvicina alla leva ed il ponte è ancora chiuso
        {
            OnPlayerNearLever?.Invoke();
            canListenInput = true;    //Inizia ad ascoltare l'eventuale input del giocatore
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isGateClosed)     //Se il player si allontana dalla leva ed il ponte è ancora chiuso
        {
            OnPlayerLeftLever?.Invoke();
            canListenInput = false;   //Smetti di ascoltare l'eventuale input del giocatore
        }
    }

    void Update()
    {
        if (isGateClosed)   //Se il ponte è chiuso
        {
            if (canListenInput)   //Se il player è nei paraggi della leva
            {
                if(Input.GetKey(KeyCode.E))
                {
                    currentTimeKeyPressed += Time.deltaTime;
                    currentTimeKeyPressed = Mathf.Clamp(currentTimeKeyPressed, 0, maxTimeToPress);
                    OnCurrentPressedKeyTimeChanged?.Invoke(currentTimeKeyPressed, maxTimeToPress);
                    if (currentTimeKeyPressed >= maxTimeToPress)
                    {
                        OnCompletedAction?.Invoke();
                        leverAnimator.SetTrigger("CanLowerLever");
                        connectedGate.OpenGate();
                        isGateClosed = false;
                        canListenInput = false;
                    }
                }
                else
                {
                    currentTimeKeyPressed = 0;
                }
            }
        }
    }
}