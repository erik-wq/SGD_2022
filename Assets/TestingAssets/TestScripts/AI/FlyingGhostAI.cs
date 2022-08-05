using Assets.TestingAssets;
using Assets.TestingAssets.TestScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingGhostAI : MonoBehaviour, IShadowEnemy
{
    #region Public
    public float Damage { get; set; }
    #endregion

    #region Serialized
    [SerializeField] private float CrossingCooldown = 5;
    [SerializeField] private float DistanceCheck = 2;
    [SerializeField] private float MaxHP = 50;
    [SerializeField] private SpriteRenderer MainSprite;
    #endregion

    #region Private
    protected GhostFollow _followScript;
    protected float _lastCrossing;
    protected bool _isLocked = false;
    protected float _hp;
    protected float _maxOpacity = 1;
    #endregion

    // Start is called before the first frame update
    protected void Start()
    {
        _hp = MaxHP;
        this.Damage = 10;
        _followScript = GetComponent<GhostFollow>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if(Time.time > _lastCrossing + CrossingCooldown)
        {
            _followScript.Cross(OnCrossFinish);
        }
    }

    private void OnCrossFinish()
    {
        _lastCrossing = Time.time;
    }

    private void AdjustOpacity()
    {
        float percentageHP = _hp / MaxHP;
        float opacity = _maxOpacity * percentageHP;
        MainSprite.color = new Color(MainSprite.color.r, MainSprite.color.g, MainSprite.color.b, opacity);
    }

    public void SetFree()
    {
        _followScript.Paused = false;
        _isLocked = false;
    }

    public bool TakeDamage(float damage, float force, Vector2 direction)
    {
        _hp -= damage;
        if(_hp < 0)
        {
            UnityEngine.Object.Destroy(this);
            return true;
        }
        else
        {
            AdjustOpacity();
            return false;
        }
    }

    public bool TakeDamage(float damage)
    {
        _hp -= damage;
        if (_hp < 0)
        {
            UnityEngine.Object.Destroy(this);
            return true;
        }
        else
        {
            AdjustOpacity();
            return false;
        }
    }

    protected void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Hope")
        {
            if (Vector2.Distance(this.transform.position, col.transform.position) < DistanceCheck)
            {
                if (!_isLocked)
                {
                    var attackResult = (col.gameObject.GetComponent<HopeAI>()).GetAttacked(this);
                    if (attackResult)
                    {
                        _followScript.Paused = true;
                        _isLocked = true;
                    }
                }
            }
            else
            {
                _followScript.SetTarget(col.transform);
            }
        }
    }
}
