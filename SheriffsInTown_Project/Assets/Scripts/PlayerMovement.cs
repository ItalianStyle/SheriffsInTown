using UnityEngine;
using Cinemachine;

namespace SheriffsInTown
{
    public class PlayerMovement : MonoBehaviour
    {
        public enum MovementState { Walk, Run, Stopped }
       
        [Header("Camera")]
        [Tooltip("Quanto velocemente gira la camera")]
        [SerializeField] float rotationSensitivity;
        [Tooltip("Rotazione con un angolo minimo raggiungibile dalla camera sull'asse X")]
        [SerializeField] float minXAngle;
        [Tooltip("Rotazione con un angolo massimo raggiungibile dalla camera sull'asse X")]
        [SerializeField] float maxXAngle;

        [Space][Header("Personaggio")]
        
        [Tooltip("Lo stato di movimento del personaggio")]
        public MovementState moveState;

        [Header("Movimento")]
        [Tooltip("Velocit� di movimento del personaggio quando cammina")]
        [SerializeField] float normalMovementSpeed = 10f;

        [Tooltip("Velocit� di movimento del personaggio quando corre")]
        [SerializeField] float runMovementSpeed = 20f;
        float currentMovementSpeed = 10f;   //Velocita' di movimento attuale del personaggio

        [Header("Rotazione")]
        [Tooltip("Quanto velocemente ruota il personaggio verso una nuova direzione")]
        [SerializeField] [Range(0f, 1f)] float smoothRotationFactor = .1f;

        [Header("Salto")]
        [Tooltip("Quanta forza usa il personaggio per saltare")]
        [SerializeField] [Min(.1f)] float jumpForce = .1f;

        Vector3 input;  //Usato per memorizzare i valori di input di movimento del giocatore
        Vector3 jumpInput = Vector3.zero;   //Utilizzato per muovere il personaggio in verticale per il salto
        Vector3 gravity = new Vector3(0, -9.81f, 0);    //Vettore di gravit� standard

        bool isShotButtonPressed = false;
        bool canJump = true;

        Camera cam; //Utilizzato per rendere il movimento dipendente dall'orientamento della camera
        CinemachineVirtualCamera virtualCamera; //Utilizzato per disabilitarlo quando si entra in pausa
        Transform followTransform; //Utilizzato per ruotare la camera intorno al giocatore (https://www.youtube.com/watch?v=537B1kJp9YQ&ab_channel=Unity)
        Vector2 rotation = Vector2.zero;
        CharacterController controller; //Utilizzato per muovere il personaggio

        private void Awake()
        {
            //Prendo i riferimenti necessari
            cam = Camera.main;
            virtualCamera = GameObject.FindGameObjectWithTag("FollowCamera").GetComponent<CinemachineVirtualCamera>();
            followTransform = transform.Find("FollowTarget");
            controller = GetComponent<CharacterController>();
        }

        private void Start()
        {
            canJump = true;
            //Chiamo il metodo quando il giocatore mette in pausa il gioco o lo riprende
            GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
            PlayerShooting.OnPlayerStartReloading += LockPlayerMovement;
            PlayerShooting.OnPlayerFinishedReloading += UnlockPlayerMovement;
            Pickup.OnHatPickupTaken += BoostMovementSpeed;
        }

        private void BoostMovementSpeed(float newMovementSpeed, float newRunMovementSpeed)
        {
            //Applica i nuovi valori potenziati
            normalMovementSpeed = newMovementSpeed;
            runMovementSpeed = newRunMovementSpeed;
        }

        private void UnlockPlayerMovement(bool isDoubleGunMode, int currentMaxCapacity)
        {
            SetPlayerMovement(MovementState.Walk);
        }

        private void LockPlayerMovement(float reloadTime)
        {
            SetPlayerMovement(MovementState.Stopped);
        }

        private void Update()
        {
            if (moveState != MovementState.Stopped)
            {
                //Salvo gli input del giocatore nel vettore
                input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

                isShotButtonPressed = Input.GetKey(KeyCode.Mouse0);
                //Meccanica di salto
                if (controller.isGrounded)
                {
                    if (canJump && Input.GetKeyDown(KeyCode.Space))
                    {
                        //Definisco il vettore massimo per saltare
                        jumpInput.y = jumpForce;
                    }

                    //Meccanica di corsa quando il giocatore preme lo shift sinistro
                    SetPlayerMovement(Input.GetKey(KeyCode.LeftShift) ? MovementState.Run : MovementState.Walk);
                }                       
            }

            #region Follow Transform rotation
            rotation.y += Input.GetAxis("Mouse X") * rotationSensitivity;
            #endregion

            #region Vertical Rotation
            rotation.x += -Input.GetAxis("Mouse Y") * rotationSensitivity;

            rotation.x = Mathf.Clamp(rotation.x, minXAngle, maxXAngle);
            followTransform.rotation = Quaternion.Euler(rotation.x, rotation.y, 0f);
            #endregion
        }

        void FixedUpdate()
        {
            /// <summary>
            /// Qui viene gestito il movimento del personaggio sul piano orizzontale con relativa rotazione
            /// </summary>

            //Determino l'orientamento di movimento del giocatore facendo ruotare di "cam.transform.eulerAngles.y" gradi il vettore di input
            Vector3 moveDir = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0) * input;

            controller.Move(moveDir.normalized * currentMovementSpeed * Time.deltaTime + gravity * Time.deltaTime);

            //Faccio ruotare il personaggio solo quando il giocatore preme uno o piu tasti di input di movimento
            if (input.magnitude >= .1f && !isShotButtonPressed)
            {
                //Stabilisco quanto e' l'angolo presente tra il movimento frontale e laterale
                float targetRotationAngle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;

                //Salvo la rotazione da raggiungere in una variabile di appoggio
                Quaternion targetRotation = Quaternion.Euler(0, targetRotationAngle, 0);

                //Smorzo la rotazione del personaggio dalla rotazione attuale a quella da raggiungere
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothRotationFactor);
            }
            //Il giocatore ruota verso la direzione della camera quando spara
            else if (isShotButtonPressed)
            {
                //Salvo la rotazione da raggiungere in una variabile di appoggio
                Quaternion targetRotation = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0);

                //Smorzo la rotazione del personaggio dalla rotazione attuale a quella da raggiungere
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1);
            }
            
            //Applico una gravita' artificiale al personaggio quando salta
            if (jumpInput.y > 0)
            {
                //Decremento il vettore di salto nel tempo
                jumpInput += gravity * Time.deltaTime;
                //Applico il vettore risultante
                controller.Move(jumpInput * Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            //Annullo l'ascolto all'evento della pausa di gioco
            GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            PlayerShooting.OnPlayerStartReloading -= LockPlayerMovement;
            PlayerShooting.OnPlayerFinishedReloading -= UnlockPlayerMovement;
            Pickup.OnHatPickupTaken -= BoostMovementSpeed;
        }

        private void SetPlayerMovement(MovementState movementState)
        {
            moveState = movementState;
            switch(moveState)
            {
                case MovementState.Walk:
                    currentMovementSpeed = normalMovementSpeed;
                    break;

                case MovementState.Run:
                    currentMovementSpeed = runMovementSpeed;
                    break;

                case MovementState.Stopped:
                    currentMovementSpeed = 0f;
                    break;
            }
        }

        private void OnGameStateChanged(GameState newGameState)
        {
            bool isGamePlaying = newGameState == GameState.Gameplay;

            //Disattivo il movimento della camera attorno al giocatore
            virtualCamera.enabled = isGamePlaying;
            //Se il gioco non e' in pausa, questo componente e' attivo
            enabled = isGamePlaying;
        }
    }
}