using System;
using System.Collections;
using UnityEngine;

namespace SheriffsInTown
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movimento")]
        [Tooltip("Velocita' di movimento del personaggio")]
        [SerializeField] float movementSpeed = 10f;

        [Header("Rotazione")]
        [Tooltip("Quanto velocemente ruota il personaggio verso una nuova direzione")]
        [SerializeField] [Range(0f, 1f)] float smoothRotationFactor = .1f;

        [Header("Salto")]
        [Tooltip("Quanta forza usa il personaggio per saltare")]
        [SerializeField] [Min(.1f)] float jumpForce = .1f;

        Vector3 input;  //Usato per memorizzare i valori di input di movimento del giocatore
        Vector3 jumpInput = Vector3.zero;   //Utilizzato per muovere il personaggio in verticale per il salto
        Vector3 gravity = new Vector3(0, -9.81f, 0);    //Vettore di gravità standard

        Camera cam; //Utilizzato per rendere il movimento dipendente dall'orientamento della camera
        CharacterController controller; //Utilizzato per muovere il personaggio

        private void Start()
        {
            //Prendo i riferimenti necessari
            cam = Camera.main;
            controller = GetComponent<CharacterController>();

            //Chiamo il metodo quando il giocatore mette in pausa il gioco o lo riprende
            GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;

            //GameManager.PlayerWonGame += () => enabled = false;
        }

        void FixedUpdate()
        {
            MovePlayer();
            Jump();
        }

        private void Jump()
        {
            if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
            {
                //Definisco il vettore massimo per saltare
                jumpInput.y = jumpForce;
            }
            if (jumpInput.y > 0)
            {
                jumpInput += gravity * Time.deltaTime;
                controller.Move(jumpInput * Time.deltaTime);
            }
        }

        private void MovePlayer()
        {
            //Salvo gli input del giocatore nel vettore
            input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

            //Determino l'orientamento di movimento del giocatore facendo ruotare di "cam.transform.eulerAngles.y" gradi il vettore di input
            Vector3 moveDir = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0) * input;

            controller.Move(moveDir.normalized * movementSpeed * Time.deltaTime + gravity * Time.deltaTime);

            //Faccio ruotare il personaggio solo quando il giocatore preme uno o piu tasti di input di movimento
            if (input.magnitude >= .1f && !Input.GetKey(KeyCode.Mouse0))
            {
                //Stabilisco quanto e' l'angolo presente tra il movimento frontale e laterale
                float targetRotationAngle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;

                //Salvo la rotazione da raggiungere in una variabile di appoggio
                Quaternion targetRotation = Quaternion.Euler(0, targetRotationAngle, 0);

                //Smorzo la rotazione del personaggio dalla rotazione attuale a quella da raggiungere
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothRotationFactor);
            }
            //Il giocatore ruota verso la direzione della camera quando spara
            else if(Input.GetKey(KeyCode.Mouse0))
            {
                //Salvo la rotazione da raggiungere in una variabile di appoggio
                Quaternion targetRotation = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0);

                //Smorzo la rotazione del personaggio dalla rotazione attuale a quella da raggiungere
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1);
            }
        }

        private void OnDestroy()
        {
            //Annullo l'ascolto all'evento della pausa di gioco
            GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;

            //GameManager.PlayerWonGame -= () => enabled = false;
        }

        private void OnGameStateChanged(GameState newGameState)
        {
            //Se il gioco non e' in pausa, questo componente e' attivo
            enabled = newGameState == GameState.Gameplay;
        }

        public void FaceCamera()
        {
            transform.rotation = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0);
        }
    }
}