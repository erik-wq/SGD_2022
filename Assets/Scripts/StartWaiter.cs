using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartWaiter : MonoBehaviour
{
    public GameObject all;
    public GameObject startCam;
    private void Update()
    {
        if (!GameData.sceneManagement.loading)
        {
            all.SetActive(true);
            startCam.SetActive(false);
            this.gameObject.SetActive(false);
        }
    }
}
