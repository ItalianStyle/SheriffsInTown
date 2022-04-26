using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leveler : MonoBehaviour
{
    public static event Action<Gate> OnPlayerNearGate = delegate { };
    public static event Action OnPlayerLeftGate = delegate { };

    [Tooltip("Riferimento al ponte a cui è collegata questa leva")]
    [SerializeField] Gate connectedGate;

    Animator leverAnimator;
    [Tooltip("Tempo richiesto al giocatore di tenere premuto il pulsante")]
    [SerializeField] float maxTimeToPress;
    
    bool canLowerLever;     //Booleana per verificare se il giocatore può abbassare la leva
    bool isGateClosed;      //Booleana che indica lo stato del ponte
    bool readyToBeLowered;      //Booleana per decrementare il contatore della leva se il giocatore non completa la discesa della leva

    //Contatore per tenere conto del tempo passato dal giocatore a tenere premuto il pulsante
    float currentTimeKeyPressed;

    private void Awake()
    {
        leverAnimator = transform.Find("Lever").GetComponent<Animator>();
    }

    private void Start()
    {
        //Stabilisco la velocità di movimento della leva in base al tempo richiesto di pressione del tasto
        leverAnimator.speed /= maxTimeToPress;

        currentTimeKeyPressed = 0;

        //Il ponte è chiuso all'inizio del gioco
        isGateClosed = true;

        //La leva è nella posizione iniziale (pronta ad essere abbassata)
        readyToBeLowered = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Se il player si avvicina alla leva ed il ponte è ancora chiuso
        if (other.CompareTag("Player") && isGateClosed)
        {
            //Inizia ad ascoltare l'eventuale input del giocatore
            canLowerLever = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Se il player si allontana dalla leva ed il ponte è ancora chiuso
        if (other.CompareTag("Player") && isGateClosed)
        {
            //Smetti di ascoltare l'eventuale input del giocatore
            canLowerLever = false;

            //Comincia a far ritornare la leva alla posizione iniziale
            readyToBeLowered = false;
        }
    }

    void Update()
    {
        //Se il ponte è chiuso ed il player è nei paraggi della leva
        if (isGateClosed && canLowerLever)
        {
            //Nell'istante in cui preme il tasto "E"
            if (Input.GetKeyDown(KeyCode.E))
            {
                //Inizia ad abbassare la leva con l'animazione
                leverAnimator.SetBool("CanLowerLever", true);
                //Non è più nella posizione iniziale
                readyToBeLowered = false;
            }

            //Per ogni frame in cui il giocatore tiene premuto il tasto "E"
            if (Input.GetKey(KeyCode.E))
            {
                //Continua a contare il tempo che passa
                currentTimeKeyPressed += Time.deltaTime;

                //Se è passato abbastanza tempo
                if (currentTimeKeyPressed >= maxTimeToPress)
                {
                    //Apri il ponte
                    connectedGate.OpenGate();
                    isGateClosed = false;

                    //Non ascoltare più l'input del giocatore
                    canLowerLever = false;
                }
            }
            //Se il giocatore rilascia il tasto "E" prima di concludere il tempo
            if (Input.GetKeyUp(KeyCode.E))
            {
                //Comincia l'animazione per ritornare alla posizione iniziale
                leverAnimator.SetBool("CanLowerLever", false);
                //Faccio decrescere il contatore per la leva
                readyToBeLowered = false;
            }
        }

        //Se la leva non è nella posizione iniziale (quindi c'è del tempo da decrementare nel contatore che è > 0)
        if (!readyToBeLowered && currentTimeKeyPressed > 0)
        {
            //Decrementa il contatore
            currentTimeKeyPressed -= Time.deltaTime;
            //Se si è esaurito il tempo
            if (currentTimeKeyPressed < 0)
            {
                //Inizializza il contatore
                currentTimeKeyPressed = 0;
                //Esci da questo if
                readyToBeLowered = true;
            }
        }
    }
}