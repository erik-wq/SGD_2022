using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private bool _paused = false;
    public GameObject menu;
    private void OnPause()
    {
        if (_paused)
        {
            menu.SetActive(false);
            Time.timeScale = 1;
        }
        else
        {
            menu.SetActive(true);
            Time.timeScale = 0;
        }
        _paused = !_paused;
    }
    public void ToMenu()
    {
        Scene[] scenes =  SceneManager.GetAllScenes();
        string cur = "";
        foreach(Scene x in scenes)
        {
            if(x.name != "Main")
            {
                cur = x.name;
                break;
            }
        }
        GameData.sceneManagement.LoadScene("Menu", cur);
    }
    public void Continue()
    {
        OnPause();
    }
}
