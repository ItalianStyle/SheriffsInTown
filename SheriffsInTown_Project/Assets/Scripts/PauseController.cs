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
        if (isGameRunning && Input.GetKeyDown(KeyCode.Escape))
        {
            GameState currentGameState = GameStateManager.Instance.CurrentGameState;
            GameState newGameState = currentGameState == GameState.Gameplay ?
                GameState.Paused
                : GameState.Gameplay;

            GameStateManager.Instance.SetState(newGameState);
        }
    }

    private void OnDisable()
    {
        //GameManager.PlayerWonGame -= () => isGameRunning = false;
    }
}