using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Hints : MonoBehaviour
{
    [SerializeField] public int Index;
    [SerializeField] public GlobalController GlobalControl;

    private bool _hintShowned = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            if(!_hintShowned)
            {
                _hintShowned = true;
                //GlobalControl.ShowHint(Index);
            }
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (!_hintShowned)
            {
                _hintShowned = true;
                //GlobalControl.ShowHint(Index);
            }
        }
    }
}
