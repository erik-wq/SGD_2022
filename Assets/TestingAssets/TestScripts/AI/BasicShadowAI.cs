using Assets.Scripts.Utils;
using Assets.TestingAssets;
using Assets.TestingAssets.TestScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicShadowAI : MonoBehaviour, IShadowEnemy
{
    public enum TargetTypes
    {
        None = 0,
        Player = 1,
        Hope = 2
    }

    public enum FollowState
    {
        Seeking = 0
    }

    #region Public
    public float Damage { get; set; }
    #endregion

    #region Serialized
    [SerializeField] private float CrossingCooldown = 5;
    [SerializeField] private float DistanceCheck = 2;
    [SerializeField] private float MaxHP = 50;
    [SerializeField] private SpriteRenderer MainSprite;
    [SerializeField] private ContactFilter2D ContactFilter;
    [SerializeField] private Collider2D AggroColider;
    [SerializeField] private Transform GraphicsTransform;

    [SerializeField] private DebugControl DebugControlComponent;

    [SerializeField] private Transform PlayerTransform;
    [SerializeField] private Transform HopeTransform;


    [SerializeField] private float HopePullRadius = 50;
    [SerializeField] private float HopeLooseAggroRadius = 50;
    [SerializeField] private float HopeLooseAgrroDelay = 4;

    [SerializeField] private float PlayerPullRadius = 50;
    [SerializeField] private float PlayerPullDelay = 50;
    [SerializeField] private float PlayerLooseAggroRadius = 50;
    [SerializeField] private float PlayerLooseAggroDelay = 50;
    #endregion

    #region Private
    protected IShadowFollow _followScript;
    protected FollowState _followState;
    protected float _lastCrossing;
    protected bool _isLocked = false;
    protected float _hp;
    protected float _maxOpacity = 1;
    protected float _rotation = 0;
    protected Rigidbody2D _rigidBody;
    protected float _startRotation = -90;

    protected float _outOfRangeTime = 0;
    protected float _switchTargetTime = 0;
    protected TargetTypes _target = TargetTypes.None;
    #endregion

    // Start is called before the first frame update
    protected void Start()
    {
        _hp = MaxHP;
        this.Damage = 10;
        _followScript = GetComponent<ShadowSeekerFollow>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _followScript.Init(GetRotation, SetRotation, ContactFilter, AggroColider, _rigidBody);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        CheckTargets();
    }

    private void CheckTargets()
    {
        var distanceFromPlayer = DistanceFromPlayer();
        var distanceFromHope = DistanceFromHope();

        if (_target != TargetTypes.None)
        {
            if (_target == TargetTypes.Hope)
            {
                if (distanceFromPlayer <= PlayerPullRadius)
                {
                    _switchTargetTime += Time.fixedDeltaTime;
                    if (DebugControlComponent != null)
                        DebugControlComponent.ShowSwitchTargetCounter(_switchTargetTime);

                    if (_switchTargetTime > PlayerPullDelay)
                    {
                        TargetPlayer();
                        _outOfRangeTime = 0;
                        _switchTargetTime = 0;
                        return;
                    }
                }
                else
                {
                    _switchTargetTime = 0;
                    _outOfRangeTime = 0;
                    if (DebugControlComponent != null)
                        DebugControlComponent.ShowSwitchTargetCounter(_switchTargetTime);
                }

                if (distanceFromHope > HopeLooseAggroRadius)
                {
                    _outOfRangeTime += Time.fixedDeltaTime;
                    if (DebugControlComponent != null)
                        DebugControlComponent.ShowLooseTargetTimer(_switchTargetTime);
                    if (_switchTargetTime > HopeLooseAgrroDelay)
                    {
                        ReturnToIdle();
                        _outOfRangeTime = 0;
                        return;
                    }
                }
                else
                {
                    _outOfRangeTime = 0;
                    if (DebugControlComponent != null)
                        DebugControlComponent.ShowLooseTargetTimer(_switchTargetTime);
                }
            }
            else if (_target == TargetTypes.Player)
            {
                if (distanceFromPlayer > PlayerLooseAggroRadius)
                {
                    _outOfRangeTime += Time.fixedDeltaTime;

                    if (DebugControlComponent != null)
                        DebugControlComponent.ShowLooseTargetTimer(_outOfRangeTime);

                    if (_outOfRangeTime > PlayerLooseAggroDelay)
                    {
                        _outOfRangeTime = 0;

                        if (distanceFromHope < HopePullRadius)
                        {
                            TargetHope();
                        }
                        else
                        {
                            ReturnToIdle();
                        }
                        return;
                    }
                }
                else
                {
                    _outOfRangeTime = 0;
                    if (DebugControlComponent != null)
                        DebugControlComponent.ShowLooseTargetTimer(_switchTargetTime);
                }
            }
        }
        else
        {
            if (distanceFromPlayer <= PlayerPullRadius)
            {
                TargetPlayer();
            }
            else if (distanceFromHope <= HopePullRadius)
            {
                TargetHope();
            }
        }
    }

    private float DistanceFromPlayer()
    {
        return Vector2.Distance(this.transform.position, PlayerTransform.position);
    }

    private float DistanceFromHope()
    {
        return Vector2.Distance(this.transform.position, HopeTransform.position);
    }

    private void TargetPlayer()
    {
        _target = TargetTypes.Player;
        _followScript.SetTarget(PlayerTransform);

        if (DebugControlComponent != null)
            DebugControlComponent.ShowTarget("Player");
    }

    private void TargetHope()
    {
        _target = TargetTypes.Hope;
        _followScript.SetTarget(HopeTransform);

        if (DebugControlComponent != null)
            DebugControlComponent.ShowTarget("Hope");
    }

    private void ReturnToIdle()
    {
        if (DebugControlComponent != null)
            DebugControlComponent.ShowTarget("None");
    }

    private void OnCrossFinish()
    {
        _lastCrossing = Time.time;
    }

    private float GetRotation()
    {
        return _rotation;
    }

    private float SetRotation(float rotation)
    {
        _rotation = rotation;
        GraphicsTransform.rotation = Quaternion.Euler(0, 0, _startRotation + rotation);
        return rotation;
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

    public bool TakeDamage(float damage, float force, Vector2 origin)
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
        //if (col.gameObject.tag == "Player")
        //{
        //    if (Vector2.Distance(this.transform.position, col.transform.position) < DistanceCheck)
        //    {
        //        if (!_isLocked)
        //        {
        //            var attackResult = (col.gameObject.GetComponent<HopeAI>()).GetAttacked(this);
        //            if (attackResult)
        //            {
        //                _followScript.Paused = true;
        //                _isLocked = true;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        _followScript.SetTarget(col.transform);
        //    }
        //}
    }
}
