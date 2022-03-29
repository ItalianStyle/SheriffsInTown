using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    //Riferimento al tasto per giocare/riprendere il gioco
    Button playBtn;

    //Riferimento al tasto per uscire dal gioco/dal menù di pausa
    Button exitBtn;

    //Riferimento alla barra HP del personaggio
    Image hpBar;

    //Riferimento al pannello della pausa
    CanvasGroup pausePanel;

    public static UI_Manager instance;

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
        SceneManager.sceneLoaded += (scene, loadSceneMode) => 
        {
            GetReferences(scene);
            SetupScene(scene);
        };
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= (scene, loadSceneMode) =>
        {
            GetReferences(scene);
            SetupScene(scene);
        };
        if(SceneManager.GetActiveScene().buildIndex == 1)
        {
            GameStateManager.Instance.OnGameStateChanged -= HandlePausePanel;
            PlayerHealthSystem.OnPlayerDamaged -= UpdateHP_Bar;
            PlayerHealthSystem.OnPlayerHealed -= UpdateHP_Bar;
        }
    }

    //Prende i riferimenti in base alla scena attiva
    private void GetReferences(Scene scene)
    {
        playBtn = GameObject.FindGameObjectWithTag("PlayBtn").GetComponent<Button>();
        exitBtn = GameObject.FindGameObjectWithTag("ExitBtn").GetComponent<Button>();
        switch (scene.buildIndex)
        {
            case 0:
                break;

            case 1:
                pausePanel = GameObject.Find("UI/PausePanel").GetComponent<CanvasGroup>();
                hpBar = GameObject.Find("UI/HUD_Panel/HP_Bar_Background/HP_Bar").GetComponent<Image>();
                break;
        }
    }

    //Prepara i componenti in base alla scena attiva
    private void SetupScene(Scene scene)
    {
        switch (scene.buildIndex)
        {
            case 0:
                //Carica la scena del gioco quando il giocatore preme il tasto "Gioca"
                playBtn.onClick.AddListener(() => SceneManager.LoadScene(1));

                //Esci dal gioco quando il giocatore preme il tasto "Esci"
                exitBtn.onClick.AddListener(() => Application.Quit());
                break;

            case 1:
                SetCanvasGroup(pausePanel, false);
                HandleCursor(GameState.Gameplay);

                //Riprendi il gioco dalla pausa quando il giocatore preme il tasto "Riprendi"
                playBtn.onClick.AddListener(() => GameStateManager.Instance.SetState(GameState.Gameplay));

                //Torna al menu' quando il giocatore preme il tasto "Esci"
                exitBtn.onClick.AddListener(() => SceneManager.LoadScene(0));

                GameStateManager.Instance.OnGameStateChanged += (newGameState) => 
                {
                    HandleCursor(newGameState);
                    HandlePausePanel(newGameState);
                } ;

                PlayerHealthSystem.OnPlayerDamaged += UpdateHP_Bar;
                PlayerHealthSystem.OnPlayerHealed += UpdateHP_Bar;
                break;
        }
    }

    private void UpdateHP_Bar(int currentHealth, int maxHealth)
    {
        hpBar.fillAmount = currentHealth / (float) maxHealth;
    }

    //Mostra e libera il cursore quando il gioco e' in pausa e viceversa
    private void HandleCursor(GameState newGameState)
    {
        bool isGamePaused = newGameState is GameState.Paused;
        Cursor.lockState =  isGamePaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isGamePaused;
    }

    //Rendi visibile o meno il pannello della pausa in base al nuovo stato di gioco
    private void HandlePausePanel(GameState newGameState)
    {
        SetCanvasGroup(pausePanel, newGameState is GameState.Paused);
    }

    public static void SetCanvasGroup(CanvasGroup canvasGroup, bool canActive)
    {
        canvasGroup.alpha = canActive ? 1f : 0f;
        canvasGroup.blocksRaycasts = canActive;
    }
}