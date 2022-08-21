using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public void Play()
    {
        GameData.sceneManagement.LoadScene("MainLevel","Menu");
    }

    public void Quit()
    {
        Application.Quit();
    }

}
