using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
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
            case 0:
                PlayerHealthSystem.OnPlayerDead += () => SceneManager.LoadScene(0);
                break;
        }
    }

    private void OnDisable()
    {
        PlayerHealthSystem.OnPlayerDead -= () => SceneManager.LoadScene(0);
    }
}
