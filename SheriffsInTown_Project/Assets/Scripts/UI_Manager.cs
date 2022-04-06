using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    //Riferimento al tasto per giocare/riprendere il gioco
    Button playBtn;

    //Riferimento al tasto per ricominciare il gioco
    Button restartBtn;

    //Riferimento al tasto per uscire dal gioco/dal menù di pausa
    Button exitBtn;

    //Riferimento alla barra HP del personaggio
    Image hpBar;

    //Riferimento ai simboli della vita del personaggio
    Image[] lifeImages;
    
    //Riferimento al pannello della pausa
    CanvasGroup pausePanel;

    //Riferimento al pannello della sconfitta
    CanvasGroup lostPanel;

    public static UI_Manager instance;

    private void OnEnable()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(instance);
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
    }

    private void OnApplicationQuit()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        //Prepara i componenti in base alla scena attiva

        playBtn = GameObject.FindGameObjectWithTag("PlayBtn").GetComponent<Button>();
        exitBtn = GameObject.FindGameObjectWithTag("ExitBtn").GetComponent<Button>();
        switch (scene.buildIndex)
        {
            case 0:
                //Carica la scena del gioco quando il giocatore preme il tasto "Gioca"
                playBtn.onClick.AddListener(call: LoadGameScene);

                //Esci dal gioco quando il giocatore preme il tasto "Esci"
                exitBtn.onClick.AddListener(call: QuitGame);
                break;

            case 1:
                //Prende i riferimenti necessari
                if(!restartBtn)
                    restartBtn = GameObject.FindGameObjectWithTag("RestartBtn").GetComponent<Button>();

                if (!pausePanel)
                    pausePanel = GameObject.Find("UI/PausePanel").GetComponent<CanvasGroup>();

                if (!lostPanel) 
                    lostPanel = GameObject.Find("UI/LostPanel").GetComponent<CanvasGroup>();

                if (!hpBar)
                    hpBar = GameObject.Find("UI/HUD_Panel/HP_Bar_Background/HP_Bar").GetComponent<Image>();

                FindLifeImages();

                SetCanvasGroup(pausePanel, false);
                SetCanvasGroup(lostPanel, false);

                HandleCursor(GameState.Gameplay);

                //Riprendi il gioco dalla pausa quando il giocatore preme il tasto "Riprendi"
                playBtn.onClick.AddListener(() => GameStateManager.Instance.SetState(GameState.Gameplay));

                //Ricomincia il gioco da zero se il giocatore preme il tasto "Ricomincia"
                restartBtn.onClick.AddListener(LoadGameScene);

                //Torna al menu' quando il giocatore preme il tasto "Esci"
                exitBtn.onClick.AddListener(LoadMenuScene);

                //Gestisci i pannelli quando il gioco passa dallo stato di gameplay ad uno che richiede una pausa
                Debug.Log("Iscritto all'evento per far apparire/sparire i pannelli");
                GameStateManager.Instance.OnGameStateChanged += HandleCursorAndPanels;

                // Aggiorna i "cuori" mostrati al giocatore quando perde vita o ricomincia il gioco
                GameManager.OnLivesChanged += UpdatePlayerLifeImages;

                //Aggiorna la barra vita in base alle varie situazioni
                PlayerHealthSystem.OnPlayerDamaged += UpdateHP_Bar;
                PlayerHealthSystem.OnPlayerHealed += UpdateHP_Bar;
                PlayerHealthSystem.OnPlayerHealthChanged += UpdateHP_Bar;
                break;
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.buildIndex == 1)
        {
            GameManager.OnLivesChanged -= UpdatePlayerLifeImages;
            Debug.Log("Disiscritto all'evento per gestire i pannelli");
            GameStateManager.Instance.OnGameStateChanged -= HandleCursorAndPanels;
            PlayerHealthSystem.OnPlayerDamaged -= UpdateHP_Bar;
            PlayerHealthSystem.OnPlayerHealed -= UpdateHP_Bar;
            PlayerHealthSystem.OnPlayerHealthChanged -= UpdateHP_Bar;
        }
    }

    private void QuitGame() => Application.Quit();

    private void LoadGameScene() => SceneManager.LoadScene(1);

    private void LoadMenuScene() => SceneManager.LoadScene(0);

    private void HandleCursorAndPanels(GameState newGameState)
    {
        Debug.Log("Lo stato del gioco è cambiato -> " + newGameState.ToString());
        HandleCursor(newGameState);
        HandlePausePanel(newGameState);
        HandleLostPanel(newGameState);
    }

    private void UpdatePlayerLifeImages(int lifeCounts)
    {
        for(int i = 0; i < lifeImages.Length; i++)
        {
            lifeImages[i].enabled = i <= (lifeCounts - 1);
        }
    }

    private void FindLifeImages()
    {
        lifeImages = new Image[3];

        for(int i = 0; i < lifeImages.Length; i++)
        {
            lifeImages[i] = GameObject.Find("LifeImage_" + i).GetComponent<Image>();
        }
    }

    private void UpdateHP_Bar(int currentHealth, int maxHealth)
    {
        //Considera i parametri se il giocatore è in vita e non sta respawnando, altrimenta riempi la barra vita
        hpBar.fillAmount = currentHealth / (float)maxHealth;        
    }

    //Mostra e libera il cursore quando il gioco e' in pausa e viceversa
    private void HandleCursor(GameState newGameState)
    {
        bool isGamePaused = newGameState is GameState.Paused || newGameState is GameState.Lost;
        Cursor.lockState =  isGamePaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isGamePaused;
    }

    //Rendi visibile o meno il pannello della pausa in base al nuovo stato di gioco
    private void HandlePausePanel(GameState newGameState)
    {
        SetCanvasGroup(pausePanel, newGameState is GameState.Paused);
    }

    //Rendi visibile o meno il pannello della sconfitta in base al nuovo stato di gioco
    private void HandleLostPanel(GameState newGameState)
    {
        SetCanvasGroup(lostPanel, newGameState is GameState.Lost);
    }

    public static void SetCanvasGroup(CanvasGroup canvasGroup, bool canActive)
    {
        canvasGroup.alpha = canActive ? 1f : 0f;
        canvasGroup.blocksRaycasts = canActive;
    }
}