using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    [Tooltip("Riferimento al tasto per giocare/riprendere il gioco")]
    [SerializeField] Button playBtn;

    [Tooltip("Riferimento al tasto per uscire dal gioco/dal menù di pausa")]
    [SerializeField] Button exitBtn;

    private void OnEnable()
    {
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
    }

    private void Start()
    {
        //Carica la scena del gioco quando il giocatore preme il tasto "Gioca"
        playBtn?.onClick.AddListener(() => SceneManager.LoadScene(1));

        //Esci dal gioco quando il giocatore preme il tasto "Esci"
        exitBtn?.onClick.AddListener(() => Application.Quit());
    }

    //Prepara i componenti in base alla scena attiva
    private void SetupScene(Scene scene)
    {
        switch(scene.buildIndex)
        {
            case 0:
                break;

            case 1:
                Cursor.lockState = CursorLockMode.Locked;
                break;
        }
    }

    //Prende i riferimenti in base alla scena attiva
    private void GetReferences(Scene scene)
    {
        switch(scene.buildIndex)
        {
            case 0:
                playBtn = GameObject.FindGameObjectWithTag("PlayBtn").GetComponent<Button>();
                exitBtn = GameObject.FindGameObjectWithTag("ExitBtn").GetComponent<Button>();
                break;

            case 1:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}