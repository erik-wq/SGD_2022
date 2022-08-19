using Assets.Scripts;
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
    private bool _isTracking = false;
    private Transform _heroTransform;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        _heroTransform = Global.Instance.PlayerTransform;
        _sprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (!_isTracking)
            return;

        if(_heroTransform == null)
            _heroTransform = Global.Instance.PlayerTransform;

        if (this.transform.position.y <= _heroTransform.position.y)
        {
            _sprite.color = new Vector4(_sprite.color.r, _sprite.color.g, _sprite.color.b, AlphaTarget);
        }
        else
        {
            _sprite.color = new Vector4(_sprite.color.r, _sprite.color.g, _sprite.color.b, 1.0F);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            _isTracking = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            _isTracking = false;
            _sprite.color = new Vector4(_sprite.color.r, _sprite.color.g, _sprite.color.b, 1.0F);
        }
    }
}
