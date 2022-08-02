using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : BaseScreen
{
    public void Play()
    {
        GameData.sceneManagement.LoadScene("Jan2Test","Menu");
    }
    public void Controls()
    {
        Hide();
        GameData.menuManager.Show<Controls>();
    }
    public void Quit()
    {
        Application.Quit();
    }
}
