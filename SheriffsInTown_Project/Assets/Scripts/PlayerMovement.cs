using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Riferimenti
    CharacterController controller;
    Camera cam;
    
    [Header("Parametri movimento")]
    [SerializeField] float _baseMoveSpeed = 10f;
    public float speedMultiplier = 1f;

    Vector3 input;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main;

        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;     //Chiamo il metodo quando il giocatore mette in pausa il gioco o lo riprende
        //GameManager.PlayerWonGame += () => enabled = false;
    }
    private void OnGameStateChanged(GameState newGameState)
    {
        enabled = newGameState == GameState.Gameplay;
    }

    void Update()
    {
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveDir = (Quaternion.Euler(0, cam.transform.eulerAngles.y, 0) * input).normalized; //Stabilisci la direzione del movimento in base alla camera
        controller.SimpleMove(moveDir * _baseMoveSpeed * speedMultiplier);    //Muovi il giocatore con la gravita' automatica attiva

        if (input.magnitude > 0)
        {
            Quaternion target = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, .1f);
        }
    }

    private void OnDestroy()
    {
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        //GameManager.PlayerWonGame -= () => enabled = false;
    }
}