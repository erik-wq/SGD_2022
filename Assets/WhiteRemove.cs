using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteRemove : MonoBehaviour
{
    [SerializeField] public GameObject WhiteObj;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnWhiteRemove()
    {
        WhiteObj.SetActive(false);
    }
}
