using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeEventHandler : MonoBehaviour
{
    private EnemyAI _mainScript;
    // Start is called before the first frame update
    void Start()
    {
        _mainScript = GetComponentInParent<EnemyAI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnAoeFire()
    {
        _mainScript.FireAoeFromAnimation();
    }

    public void OnMeleeDamge()
    {
        _mainScript.DoMeleeDamageFromAnimation();
    }
}
