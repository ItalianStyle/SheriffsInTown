using UnityEngine;

public class PauseController : MonoBehaviour
{
    bool isGameRunning = true;

    private void OnEnable()
    {
        isGameRunning = true;
        //GameManager.PlayerWonGame += () => isGameRunning = false;
    }

    void Update()
    {
        //Metti in pausa il gioco se non si e' gia' in pausa ed il giocatore ha premuto il tasto Esc
        if (isGameRunning && Input.GetKeyDown(KeyCode.Escape))
        {
            GameState currentGameState = GameStateManager.Instance.CurrentGameState;

            //Invertiamo lo stato di gioco
            GameState newGameState = currentGameState == GameState.Gameplay ? GameState.Paused : GameState.Gameplay;

            GameStateManager.Instance.SetState(newGameState);
        }
    }

    private void OnDisable()
    {
        //GameManager.PlayerWonGame -= () => isGameRunning = false;
    }
}