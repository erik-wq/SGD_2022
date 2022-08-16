using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostEventHandler : MonoBehaviour
{
    private BasicShadowAI _mainScript;
    private ShadowCircleFollow _circleScript;
    // Start is called before the first frame update
    void Start()
    {
        _mainScript = GetComponentInParent<BasicShadowAI>();
        _circleScript = GetComponentInParent<ShadowCircleFollow>();
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

    public void OnAttackEnds()
    {
        _circleScript.OnAttackEnds();
    }
}
