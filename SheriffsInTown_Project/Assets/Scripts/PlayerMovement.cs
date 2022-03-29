using UnityEngine;
using Cinemachine;

namespace SheriffsInTown
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movimento")]
        [Tooltip("Velocita' di movimento del personaggio")]
        [SerializeField] float movementSpeed = 10f;
        float movementSpeedReference;

        [Header("Rotazione")]
        [Tooltip("Quanto velocemente ruota il personaggio verso una nuova direzione")]
        [SerializeField] [Range(0f, 1f)] float smoothRotationFactor = .1f;

        [Header("Salto")]
        [Tooltip("Quanta forza usa il personaggio per saltare")]
        [SerializeField] [Min(.1f)] float jumpForce = .1f;

        Vector3 input;  //Usato per memorizzare i valori di input di movimento del giocatore
        Vector3 jumpInput = Vector3.zero;   //Utilizzato per muovere il personaggio in verticale per il salto
        Vector3 gravity = new Vector3(0, -9.81f, 0);    //Vettore di gravità standard

        bool isShotButtonPressed = false;
        bool canMove = true;

        Camera cam; //Utilizzato per rendere il movimento dipendente dall'orientamento della camera
        CinemachineVirtualCamera virtualCamera; //Utilizzato per disabilitarlo quando si entra in pausa

        CharacterController controller; //Utilizzato per muovere il personaggio

        private void Start()
        {
            //Prendo i riferimenti necessari
            cam = Camera.main;
            virtualCamera = GameObject.Find("FollowPlayerCamera").GetComponent<CinemachineVirtualCamera>();
            controller = GetComponent<CharacterController>();

            movementSpeedReference = movementSpeed;

            SpecialSkill.OnActivatedSkill += (skill) => SetPlayerMovement(skill.NewMovementSpeed);
            SpecialSkill.OnFinishedSkill += () => SetPlayerMovement(movementSpeedReference);

            //Chiamo il metodo quando il giocatore mette in pausa il gioco o lo riprende
            GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;

            //GameManager.PlayerWonGame += () => enabled = false;
            PlayerShooting.OnPlayerStartReloading += () => SetPlayerMovement(0);
            PlayerShooting.OnPlayerFinishedReloading += () => SetPlayerMovement(movementSpeedReference);
        }

        private void Update()
        {
            if (canMove)
            {
                //Salvo gli input del giocatore nel vettore
                input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

                isShotButtonPressed = Input.GetKey(KeyCode.Mouse0);
                //Meccanica di salto
                if (controller.isGrounded)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        //Definisco il vettore massimo per saltare
                        jumpInput.y = jumpForce;
                    }
                }
            }
        }

        void FixedUpdate()
        {
            /// <summary>
            /// Qui viene gestito il movimento del personaggio sul piano orizzontale con relativa rotazione
            /// </summary>

            //Determino l'orientamento di movimento del giocatore facendo ruotare di "cam.transform.eulerAngles.y" gradi il vettore di input
            Vector3 moveDir = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0) * input;

            controller.Move(moveDir.normalized * movementSpeed * Time.deltaTime + gravity * Time.deltaTime);

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
            SpecialSkill.OnActivatedSkill -= (skill) => SetPlayerMovement(skill.NewMovementSpeed);
            SpecialSkill.OnFinishedSkill -= () => SetPlayerMovement(movementSpeedReference);

            //Annullo l'ascolto all'evento della pausa di gioco
            GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;

            //GameManager.PlayerWonGame -= () => enabled = false;

            PlayerShooting.OnPlayerStartReloading -= () => SetPlayerMovement(0);
            PlayerShooting.OnPlayerFinishedReloading -= () => SetPlayerMovement(movementSpeedReference);
        }

        private void SetPlayerMovement(float newMovementSpeed)
        {
            canMove = newMovementSpeed > 0;
            movementSpeed = newMovementSpeed;
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