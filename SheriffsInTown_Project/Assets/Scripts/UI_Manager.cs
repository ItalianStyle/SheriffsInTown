using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    [SerializeField] Button playBtn;
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
        playBtn.onClick.AddListener(() =>
        {
            Debug.Log("Bottone cliccato");
            SceneManager.LoadScene(1);
        });


        exitBtn.onClick.AddListener(() => Application.Quit());
    }

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
