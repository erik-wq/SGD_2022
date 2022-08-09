using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransparencyControl : MonoBehaviour
{
    #region Serialized
    [SerializeField] private float AlphaTarget = 0.8f;
    #endregion

    #region Private
    private SpriteRenderer _sprite;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        _sprite = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            _sprite.color = new Vector4(_sprite.color.r, _sprite.color.g, _sprite.color.b, AlphaTarget);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            _sprite.color = new Vector4(_sprite.color.r, _sprite.color.g, _sprite.color.b, 1.0F);
        }
    }
}
