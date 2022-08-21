using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] public GameObject LoadingScreen;
    private bool _isLoading = false;

    public void Play()
    {
        LoadingScreen.SetActive(true);
        SceneManager.LoadScene("MainLevel");
    }

    public void Quit()
    {
        Application.Quit();
    }

    void Update()
    {

    }
}
