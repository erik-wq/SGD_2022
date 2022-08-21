using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStart : MonoBehaviour
{
    public GameObject game;
    public GameObject enemies;
    public GameObject cam;
    public Image image;
    public Sprite[] hints;
    public GameObject UI;

    int index = 0;
    bool active = false;
    private void Update()
    {
        if (!active)
        {
            return;
        }
        if (Input.anyKeyDown)
        {
            Debug.Log("change");
            index += 1;
            if (index < hints.Length)
            {
                Debug.Log("succes");
                image.sprite = hints[index];
            }else if(index >= hints.Length)
            {
                image.gameObject.SetActive(false);
                game.SetActive(true);
                Time.timeScale = 1;
                active = false;
            }
        }
    }
    private void Init()
    {
        Time.timeScale = 0;
        image.sprite = hints[0];
        cam.SetActive(false);
        enemies.SetActive(false);
        image.gameObject.SetActive(true);
        UI.SetActive(true);
        active = true;
    }
}
