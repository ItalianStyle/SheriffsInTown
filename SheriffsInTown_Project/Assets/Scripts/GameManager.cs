using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    //Riferimento al player
    GameObject player;

    //Vite a disposizione del giocatore prima di perdere la partita
    int totalLives = 3;

    private void OnEnable()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        switch(scene.buildIndex)
        {
            case 1:
                player = GameObject.FindGameObjectWithTag("Player");
                PlayerHealthSystem.OnPlayerDead += HandlePlayerDeath;
                break;
        }
    }

    private void Start()
    {
        //Inizializzo lo stato di gioco quando comincia il gioco
        GameStateManager.Instance.SetState(GameState.Gameplay);
    }

    void HandlePlayerDeath()
    {
        totalLives--;
        if (totalLives > 0)
        {
            //Respawna
            
        }
        else
        {
            //Gioco perso
        }
    }

    private void OnDisable()
    {
        PlayerHealthSystem.OnPlayerDead -= () => SceneManager.LoadScene(0);
    }
}
