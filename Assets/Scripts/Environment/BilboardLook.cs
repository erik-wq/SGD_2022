using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BilboardLook : MonoBehaviour
{
    [SerializeField] private Transform MainCamera;
    private Transform _transform;
    // Start is called before the first frame update
    void Start()
    {
        _transform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(MainCamera.transform);
    }
}
