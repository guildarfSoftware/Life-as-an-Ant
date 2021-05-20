using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void OnPlayClick()
    {
        SceneManager.LoadScene("InGame"); 
    }

    public void OnInstructionsClick()
    {
        SceneManager.LoadScene("Instructions");
    }

    public void OnExitClick()
    {
        Application.Quit();
    }

}
