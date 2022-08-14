using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostEventHandler : MonoBehaviour
{
    private BasicShadowAI _mainScript;
    // Start is called before the first frame update
    void Start()
    {
        _mainScript = GetComponentInParent<BasicShadowAI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDeathStart()
    {
        _mainScript.OnDeathStarts();
    }

    public void OnDeathEnd()
    {
        _mainScript.OnDeathFromAnimation();
    }
}
