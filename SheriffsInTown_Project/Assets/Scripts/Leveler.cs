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

    Transform lever;
    Animator leverAnimator;
    [Tooltip("Tempo richiesto al giocatore di tenere premuto il pulsante")]
    [SerializeField] float maxTimeToPress;
    float currentTimeKeyPressed;        //Contatore per tenere conto del tempo passato dal giocatore a tenere premuto il pulsante

    bool canListenInput;     //Booleana per verificare se il giocatore è nelle vicinanze della leva
    bool isGateClosed;      //Booleana che indica lo stato del ponte
    bool readyToBeLowered;      //Booleana per decrementare il contatore della leva se il giocatore non completa la discesa della leva

    Coroutine lowerLever = null;
    Coroutine raiseLever = null;

    private void Awake()
    {
        lever = transform.Find("Lever");
        //leverAnimator = lever.GetComponent<Animator>();
    }

    private void Start()
    {
        //leverAnimator.speed /= maxTimeToPress;  //Stabilisco la velocità di movimento della leva in base al tempo richiesto di pressione del tasto
        currentTimeKeyPressed = 0;

        isGateClosed = true;    //Il ponte è chiuso all'inizio del gioco

        readyToBeLowered = true;    //La leva è nella posizione iniziale (pronta ad essere abbassata)
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isGateClosed)     //Se il player si avvicina alla leva ed il ponte è ancora chiuso
            canListenInput = true;    //Inizia ad ascoltare l'eventuale input del giocatore
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && isGateClosed)     //Se il player si allontana dalla leva ed il ponte è ancora chiuso
        {
            canListenInput = false;   //Smetti di ascoltare l'eventuale input del giocatore
            //leverAnimator.SetBool("CanLowerLever", false);  //Comincia l'animazione per ritornare alla posizione iniziale
            readyToBeLowered = false;   //Disabilita la leva
        }
    }

    /*void Update()
    {
        if (isGateClosed)   //Se il ponte è chiuso
        {       
            if (canListenInput)   //Se il player è nei paraggi della leva
            {       
                if (Input.GetKeyDown(KeyCode.E))    //Nell'istante in cui preme il tasto "E"
                {       
                    leverAnimator.SetBool("CanLowerLever", true);   //Inizia ad abbassare la leva con l'animazione   
                    readyToBeLowered = false;   //Non è più nella posizione iniziale
                }  
                else if (Input.GetKey(KeyCode.E))   //Per ogni frame in cui il giocatore tiene premuto il tasto "E"
                {  
                    currentTimeKeyPressed += Time.deltaTime;    //Continua a contare il tempo che passa
                    
                    if (currentTimeKeyPressed >= maxTimeToPress)    //Se è passato abbastanza tempo
                    {                         
                        connectedGate.OpenGate();   //Apri il ponte
                        isGateClosed = false;
                        canListenInput = false;   //Non ascoltare più l'input del giocatore
                    }
                }
                else if (Input.GetKeyUp(KeyCode.E) && currentTimeKeyPressed < maxTimeToPress)   //Se il giocatore rilascia il tasto "E" prima di concludere il tempo
                {
                    leverAnimator.SetBool("CanLowerLever", false);  //Comincia l'animazione per ritornare alla posizione iniziale
                    readyToBeLowered = false;   //Faccio decrescere il contatore per la leva
                }
                else if(currentTimeKeyPressed > 0)  //Se la leva non è nella posizione iniziale
                {
                    currentTimeKeyPressed -= Time.deltaTime;    //Decrementa il contatore

                    if (currentTimeKeyPressed < 0)  //Se si è esaurito il tempo
                    {
                        currentTimeKeyPressed = 0;  //Inizializza il contatore
                        readyToBeLowered = true;    //Esci da questo if
                    }
                } 
            }
            //Se il giocatore non è nei paraggi e la leva non è nella posizione iniziale (quindi c'è del tempo da decrementare nel contatore che è > 0) 
            else if (!readyToBeLowered && currentTimeKeyPressed > 0)
            {
                currentTimeKeyPressed -= Time.deltaTime;    //Decrementa il contatore
                
                if (currentTimeKeyPressed < 0)  //Se si è esaurito il tempo
                {
                    currentTimeKeyPressed = 0;  //Inizializza il contatore
                    readyToBeLowered = true;    //Esci da questo if
                }
            }
        }
    }*/

    void Update()
    {
        if (isGateClosed)   //Se il ponte è chiuso
        {
            if (canListenInput)   //Se il player è nei paraggi della leva
            {
                if(Input.GetKeyDown(KeyCode.E))
                {
                    if (raiseLever != null)
                    {
                        StopCoroutine(raiseLever);
                        raiseLever = null;
                    }
                    lowerLever = StartCoroutine(LerpFunction(true));
                }
                else if(Input.GetKeyUp(KeyCode.E))
                {
                    if (lowerLever != null)
                    {
                        StopCoroutine(lowerLever);
                        lowerLever = null;
                    }
                    raiseLever = StartCoroutine(LerpFunction(false));
                }
            }
        }
    }
    IEnumerator LerpFunction(bool isDownDirection)
    {
        Quaternion startValue = isDownDirection ? Quaternion.Euler(65, 0, 0) : Quaternion.Euler(-65, 0, 0);
        Quaternion endValue = isDownDirection ? Quaternion.Euler(-65, 0, 0) : Quaternion.Euler(65, 0, 0);
        float delta = currentTimeKeyPressed / maxTimeToPress;
        if (delta > 0)
        {
            //Bisogna trovare il valore simmetrico del delta quando si cambia direzione per non flippare la leva
            //Esempio: se il delta è 0.1 nella fase di discesa, questa corrisponde a 0.9 in fase di salita
            float offset = .5f - delta;
            if (offset > 0)
            {
                delta = delta + offset;
            }
                    
        }

        while (currentTimeKeyPressed >= 0 && currentTimeKeyPressed < maxTimeToPress)
        {
            lever.localRotation = Quaternion.Lerp(startValue, endValue, currentTimeKeyPressed / maxTimeToPress);

            if (isDownDirection)
                currentTimeKeyPressed += Time.deltaTime;
            else
                currentTimeKeyPressed -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        lever.localRotation = endValue;
        currentTimeKeyPressed = isDownDirection ? maxTimeToPress : 0;
    }
}