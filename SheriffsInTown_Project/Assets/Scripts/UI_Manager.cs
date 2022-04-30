using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UI_Manager : MonoBehaviour
{
    //Riferimento al tasto per giocare/riprendere il gioco
    Button playBtn;

    //Riferimento al tasto per ricominciare il gioco
    Button restartBtn;

    //Riferimento al tasto per uscire dal gioco/dal menù di pausa
    Button exitBtn;

    [Tooltip("Assegna dal project tab l'icona della stella brillante")]
    [SerializeField] Sprite specialSkillActivated;
    [Tooltip("Assegna dal project tab l'icona della stella normale")]
    [SerializeField] Sprite specialSkillNormal;

    [Tooltip("Colore che possiede la UI del proiettile non utilizzato")]
    [SerializeField] Color bulletColor;


    Image backgroundGunReload_L;    //Riferimento all'immagine di background per la ricarica della pistola sinistra
    Image backgroundGunReload_R;    //Riferimento all'immagine di background per la ricarica della pistola destra

    Image gunReload_L;  //Riferimento all'immagine della ricarica della pistola sinistra
    Image gunReload_R;  //Riferimento all'immagine delle ricarica della pistola destra

    TMP_Text munitionsText;

    Image specialSkillImage;    //Riferimento all'immagine dell'abilità speciale
    
    Image hpBar;    //Riferimento alla barra HP del personaggio

    Image actionBar;    //Riferimento alla barra azione

    //Riferimento ai simboli della vita del personaggio
    Image[] lifeImages;

    Image[] bulletImages;  //Riferimento alle immagini dei proiettili per le munizioni

    //Riferimento al pannello della pausa
    CanvasGroup pausePanel;

    //Riferimento al pannello della sconfitta
    CanvasGroup lostPanel;

    CanvasGroup actionPanel;    //Riferimento al pannello azione

    CanvasGroup hudPanel;   //Riferimento all'HUD

    bool isActionPanelActive;

    readonly string HUD_PanelString = "UI/HUD_Panel";

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

                if (!actionPanel)
                    actionPanel = GameObject.Find("UI/ActionPanel").GetComponent<CanvasGroup>();

                if (!hudPanel)
                    hudPanel = GameObject.Find(HUD_PanelString).GetComponent<CanvasGroup>();

                if (!hpBar)
                    hpBar = hudPanel.transform.Find("HP_Bar_Background/HP_Bar").GetComponent<Image>();

                if (!actionBar)
                    actionBar = actionPanel.transform.Find("Background").GetComponent<Image>();

                if (!specialSkillImage)
                    specialSkillImage = hudPanel.transform.Find("AbilitaSpecialeBackground/AbilitaSpecialeImmagine").GetComponent<Image>();

                if (!backgroundGunReload_L)
                    backgroundGunReload_L = hudPanel.transform.Find("MunizioniPanel/BackgroundMunizioniPistolaSX").GetComponent<Image>();

                if (!backgroundGunReload_R)
                    backgroundGunReload_R = hudPanel.transform.Find("MunizioniPanel/BackgroundMunizioniPistolaDX").GetComponent<Image>();

                if (!gunReload_L)
                    gunReload_L = backgroundGunReload_L.transform.Find("ImmagineMunizioniPistolaSX").GetComponent<Image>();

                if (!gunReload_R)
                    gunReload_R = backgroundGunReload_R.transform.Find("ImmagineMunizioniPistolaDX").GetComponent<Image>();

                if (!munitionsText)
                    munitionsText = hudPanel.transform.Find("MunizioniPanel/MunizioniText").GetComponent<TMP_Text>();

                actionBar.fillAmount = 0;
                gunReload_L.fillAmount = 1;
                gunReload_R.fillAmount = 1;

                FindLifeImages();
                FindBulletImages();

                SetCanvasGroup(pausePanel, false);
                SetCanvasGroup(lostPanel, false);
                DisableActionPanel();
                SetCanvasGroup(hudPanel, true);

                HandleCursor(GameState.Gameplay);

                //Riprendi il gioco dalla pausa quando il giocatore preme il tasto "Riprendi"
                playBtn.onClick.AddListener(() => GameStateManager.Instance.SetState(GameState.Gameplay));

                //Ricomincia il gioco da zero se il giocatore preme il tasto "Ricomincia"
                restartBtn.onClick.AddListener(LoadGameScene);

                //Torna al menu' quando il giocatore preme il tasto "Esci"
                exitBtn.onClick.AddListener(LoadMenuScene);

                //Gestisci i pannelli quando il gioco passa dallo stato di gameplay ad uno che richiede una pausa
                GameStateManager.Instance.OnGameStateChanged += HandleCursorAndPanels;

                // Aggiorna i "cuori" mostrati al giocatore quando perde vita o ricomincia il gioco
                GameManager.OnLivesChanged += UpdatePlayerLifeImages;

                //Aggiorna la barra vita in base alle varie situazioni
                PlayerHealthSystem.OnPlayerDamaged += UpdateHP_Bar;
                PlayerHealthSystem.OnPlayerHealed += UpdateHP_Bar;
                PlayerHealthSystem.OnPlayerHealthChanged += UpdateHP_Bar;

                PlayerShooting.OnPlayerChangedFireMode += UpdateGunsUI_Appearance;
                PlayerShooting.OnShotFired += UpdateGunsUI_Values;
                PlayerShooting.OnPlayerFinishedReloading += FillGunsUI;

                //Interagisci con il pannello azione quando il giocatore interagisce nell'area della leva
                Lever.OnPlayerNearLever += EnableActionPanel;
                Lever.OnPlayerLeftLever += DisableActionPanel;
                Lever.OnCompletedAction += DisableActionPanel;
                Lever.OnCurrentPressedKeyTimeChanged += SetActionBarFillAmount;

                //Modifica il pannello della barra stamina dell'abilità speciale quando cambia il suo stato
                SpecialSkill.OnSpecialSkillBarChangedValue += HandleSpecialSkillIcon;
                SpecialSkill.OnActivatedSkill += HandleSpecialSkillActivation;
                SpecialSkill.OnFinishedSkill += HandleSpecialSkillDeactivation;

                break;
        }
    }

    private void FillGunsUI(bool isDoubleGunMode, int currentMaxCapacity)
    {
        for (int i = 0; i < currentMaxCapacity; i++)
            bulletImages[i].color = bulletColor;

        if(isDoubleGunMode)
        {
            gunReload_L.fillAmount = 1f;
        }
        gunReload_R.fillAmount = 1f;
        munitionsText.text = $"{currentMaxCapacity} / {currentMaxCapacity}";
    }

    //Quando viene sparato un colpo gestisci la UI delle munizioni e ricarica
    private void UpdateGunsUI_Values(bool isDoubleGunMode, float currentCapacity, float maxCapacity)
    {
        if(isDoubleGunMode)
        {
            gunReload_L.fillAmount = currentCapacity / maxCapacity;
        }
        gunReload_R.fillAmount = currentCapacity / maxCapacity;
        bulletImages[(int)currentCapacity].color = Color.gray;
        
        munitionsText.text = $"{currentCapacity} / {maxCapacity}";
    }

    //In base alla modalità di sparo scelta, vengono aggiunte o tolte le icone necessarie
    private void UpdateGunsUI_Appearance(bool isDoubleGunMode)
    {
        //Abilita/Disabilita l'icona della pistola sinistra
        backgroundGunReload_L.enabled = isDoubleGunMode;
        gunReload_L.enabled = isDoubleGunMode;

        //Abilita/disabilita i proiettili per la pistola secondaria
        for(int i = bulletImages.Length / 2; i < bulletImages.Length; i++)
        {
            bulletImages[i].enabled = isDoubleGunMode;
            if (isDoubleGunMode)
                bulletImages[i].color = Color.gray;
        }
    }

    private void HandleSpecialSkillDeactivation()
    {
        specialSkillImage.sprite = specialSkillNormal;
    }

    private void HandleSpecialSkillActivation(SpecialSkill obj)
    {
        specialSkillImage.sprite = specialSkillActivated;
    }

    private void HandleSpecialSkillIcon(float currentAmount, float maxAmount)
    {
        specialSkillImage.fillAmount = currentAmount / maxAmount;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.buildIndex == 1)
        {
            GameManager.OnLivesChanged -= UpdatePlayerLifeImages;
            GameStateManager.Instance.OnGameStateChanged -= HandleCursorAndPanels;
            PlayerHealthSystem.OnPlayerDamaged -= UpdateHP_Bar;
            PlayerHealthSystem.OnPlayerHealed -= UpdateHP_Bar;
            PlayerHealthSystem.OnPlayerHealthChanged -= UpdateHP_Bar;
            PlayerShooting.OnPlayerChangedFireMode -= UpdateGunsUI_Appearance;
            PlayerShooting.OnShotFired -= UpdateGunsUI_Values;
            Lever.OnPlayerNearLever -= EnableActionPanel;
            Lever.OnPlayerLeftLever -= DisableActionPanel;
            Lever.OnCompletedAction -= DisableActionPanel;
            Lever.OnCurrentPressedKeyTimeChanged -= SetActionBarFillAmount;
            SpecialSkill.OnSpecialSkillBarChangedValue -= HandleSpecialSkillIcon;
            SpecialSkill.OnActivatedSkill -= HandleSpecialSkillActivation;
            SpecialSkill.OnFinishedSkill -= HandleSpecialSkillDeactivation;
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
        HandleActionPanel(newGameState);
    }

    private void UpdatePlayerLifeImages(int lifeCounts)
    {
        for(int i = 0; i < lifeImages.Length; i++)
        {
            lifeImages[i].color = i <= (lifeCounts - 1)? Color.white : Color.black;
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

    private void FindBulletImages()
    {
        bulletImages = new Image[12];

        for (int i = 0; i < bulletImages.Length; i++)
        {
            bulletImages[i] = hudPanel.transform.Find("MunizioniPanel/PallottolePanel/Pallottola_" + i).GetComponent<Image>();
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

    private void HandleActionPanel(GameState newGameState)
    {
        SetCanvasGroup(actionPanel, isActionPanelActive && newGameState is GameState.Gameplay);
    }

    void EnableActionPanel()
    {
        isActionPanelActive = true;
        SetCanvasGroup(actionPanel, true);
    }

    void DisableActionPanel()
    {
        actionBar.fillAmount = 0f;
        SetCanvasGroup(actionPanel, false);
        isActionPanelActive = false;
    }

    void SetActionBarFillAmount(float currentAmount, float totalAmount)
    {
        actionBar.fillAmount = currentAmount / totalAmount;
    }

    public void SetReloadBarFillAmount(bool isDoubleGunMode, float currentAmount, float maxAmount)
    {
        if (isDoubleGunMode)
            gunReload_L.fillAmount = currentAmount / maxAmount;
        gunReload_R.fillAmount = currentAmount / maxAmount;
    }

    public static void SetCanvasGroup(CanvasGroup canvasGroup, bool canActive)
    {
        canvasGroup.alpha = canActive ? 1f : 0f;
        canvasGroup.blocksRaycasts = canActive;
    }
}