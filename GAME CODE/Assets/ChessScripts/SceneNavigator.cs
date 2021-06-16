using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneNavigator : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("GameMode");
    }


    public void Exit()
    {
        Application.Quit();
        Debug.Log("Quit!!");
    }

    public void PlayerVsPlayer()
    {
        SceneManager.LoadScene("main_Scene");
    }


    public void Online()
    {
        SceneManager.LoadScene("Game");
    }

    public void GameModeBack()
    {
        SceneManager.LoadScene("MainMenu");
    }


    public void OnlineBack()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Retry()
    {
        SceneManager.LoadScene("main_Scene");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void GameBack()
    {
        SceneManager.LoadScene("GameMode");
    }



}
