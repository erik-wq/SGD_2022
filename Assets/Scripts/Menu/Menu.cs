using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void Play()
    {
        GameData.sceneManagement.LoadScene("Jan2Test", "Menu");
    }
    public void Quit()
    {
        Application.Quit();
    }
}
