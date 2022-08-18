using Assets.Scripts;
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
        Seeking = 0,
        Circleing = 1,
        Idle = 3
    }

    #region Public
    public float Damage { get; set; }
    #endregion

    #region Serialized
    [SerializeField] private float CrossingCooldown = 5;
    [SerializeField] private float DistanceCheck = 2;
    [SerializeField] private float MaxHP = 50;
    [SerializeField] private float MinimalAlpha = 0.5f;
    [SerializeField] private SpriteRenderer MainSprite;
    [SerializeField] private ContactFilter2D ContactFilter;
    [SerializeField] private Collider2D AggroColider;
    [SerializeField] private Transform GraphicsTransform;
    [SerializeField] private Animator AttackAnimator;

    [SerializeField] private DebugControl DebugControlComponent;

    [SerializeField] private Transform PlayerTransform;
    [SerializeField] private Transform HopeTransform;
    [SerializeField] private GameObject Light;
    [SerializeField] private TrailRenderer Trail;

    [SerializeField] private float HopePullRadius = 50;
    [SerializeField] private float HopeLooseAggroRadius = 50;
    [SerializeField] private float HopeLooseAgrroDelay = 4;

    [SerializeField] private float PlayerPullRadius = 50;
    [SerializeField] private float PlayerPullDelay = 50;
    [SerializeField] private float PlayerLooseAggroRadius = 50;
    [SerializeField] private float PlayerLooseAggroDelay = 50;

    [SerializeField] private float CircleRadiusLost = 5;
    #endregion

    #region Private
    protected IShadowFollow _seekingFollow;
    protected ShadowCircleFollow _circleFollow;
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

    protected float _circleRadius;
    protected float _circleRadiusLostDistance;
    protected bool _isDead = false;
    protected event Action _onDeath;

    #endregion

    // Start is called before the first frame update
    protected void Start()
    {
        _hp = MaxHP;
        this.Damage = 10;
        _seekingFollow = GetComponent<ShadowSeekerFollow>();
        _circleFollow = GetComponent<ShadowCircleFollow>();

        _circleFollow.Init(GetRotation, SetRotation);

        _circleFollow.Paused = true;
        _rigidBody = GetComponent<Rigidbody2D>();
        _seekingFollow.Init(GetRotation, SetRotation, ContactFilter, AggroColider, _rigidBody);

        _circleRadiusLostDistance = _circleRadius + CircleRadiusLost;

        if ( HopeTransform == null || PlayerTransform == null)
        {
            //HopeAIScript = Global.Instance.HopeScript;
            HopeTransform = Global.Instance.HopeTransform;
            //Player = Global.Instance.PlayerScript;
            PlayerTransform = Global.Instance.PlayerTransform;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        if (_isDead)
            return;

        CheckTargets();
        CheckDistance();
    }

    public void RegisterOnDeath(Action action)
    {
        _onDeath += action;
    }

    private void CheckDistance()
    {
        var distance = DistanceFromTarget();
        if (distance < 0)
            return;

        if (_followState != FollowState.Idle)
        {
            RegisterFollow();
        }

        if (distance < _circleRadius && _followState == FollowState.Seeking)
        {
            SwitchToCircleFollow();
        }
        else if (distance > _circleRadiusLostDistance && _followState == FollowState.Circleing)
        {
            SwitchToSeeking();
        }
    }

    private void SwitchToCircleFollow()
    {
        _followState = FollowState.Circleing;
        _seekingFollow.Paused = true;
        _circleFollow.Paused = false;
        _circleFollow.InitCircle();
        _circleFollow.SetTarget(GetTarget());
    }

    private void SwitchToSeeking()
    {
        _followState = FollowState.Seeking;
        _seekingFollow.Paused = false;
        _circleFollow.Paused = true;
        _seekingFollow.SetTarget(GetTarget());
    }

    private Transform GetTarget()
    {
        if (_target == TargetTypes.Hope)
        {
            return HopeTransform;
        }
        else if (_target == TargetTypes.Player)
        {
            return PlayerTransform;
        }

        return null;
    }

    public void OnDeathStarts()
    {
        MainSprite.enabled = false;
    }

    private float DistanceFromTarget()
    {
        if (_target == TargetTypes.Hope)
        {
            return Vector2.Distance(this.transform.position, HopeTransform.position);
        }
        else if (_target == TargetTypes.Player)
        {
            return Vector2.Distance(this.transform.position, PlayerTransform.transform.position);
        }
        else
        {
            return -1;
        }
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
        _seekingFollow.SetTarget(PlayerTransform);
        _circleFollow.SetTarget(PlayerTransform);

        RegisterFollow();

        if (DebugControlComponent != null)
            DebugControlComponent.ShowTarget("Player");
    }

    private void RegisterFollow()
    {
        if (_circleRadius <= 0)
        {
            var radius = GhostsControlsSingleton.GetInstance().RegisterMe(_circleFollow, _target);
            if (radius > 0)
            {
                _circleRadius = radius;
                _circleFollow.CircleRadius = radius;
                _circleRadiusLostDistance = _circleRadius + CircleRadiusLost;
            }
        }
    }

    private void TargetHope()
    {
        _target = TargetTypes.Hope;
        _seekingFollow.SetTarget(HopeTransform);
        _circleFollow.SetTarget(HopeTransform);

        RegisterFollow();

        if (DebugControlComponent != null)
            DebugControlComponent.ShowTarget("Hope");
    }

    private void ReturnToIdle()
    {
        if (DebugControlComponent != null)
            DebugControlComponent.ShowTarget("None");

        GhostsControlsSingleton.GetInstance().RemoveMe(_circleFollow);
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
        //GraphicsTransform.rotation = Quaternion.Euler(0, 0, _startRotation + rotation);
        this.transform.rotation = Quaternion.Euler(0, 0, _startRotation + rotation);
        return rotation;
    }

    private void AdjustOpacity()
    {
        float percentageHP = _hp / MaxHP;
        float opacity = ((_maxOpacity - MinimalAlpha) * percentageHP) + MinimalAlpha;
        MainSprite.color = new Color(MainSprite.color.r, MainSprite.color.g, MainSprite.color.b, opacity);
        Trail.startColor = new Color(Trail.startColor.r, Trail.startColor.g, Trail.startColor.b, opacity);
    }

    public void SetFree()
    {
        _seekingFollow.Paused = false;
        _isLocked = false;
    }

    public bool TakeDamage(float damage, float force, Vector2 origin)
    {
        _hp -= damage;
        DamageToFollow(damage);
        if (_hp < 0)
        {
            OnDeath();
            GhostsControlsSingleton.GetInstance().RemoveMe(_circleFollow);
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
        return TakeDamage(damage, 0, Vector2.zero);
    }

    private void DamageToFollow(float damage)
    {
        _circleFollow.TakeDamage(damage);
    }

    private void OnDeath()
    {
        if (!_isDead)
        {
            _isDead = true;
            AttackAnimator.Play("GhostDeath");
            _circleFollow.enabled = false;
            _seekingFollow.Paused = true;

            if (_onDeath != null)
                _onDeath();
        }
    }

    public void OnFireTrigger()
    {
        //TODO
    }

    public void OnDeathFromAnimation()
    {
        MainSprite.enabled = false;
        Trail.enabled = false;
        Light.SetActive(true);
        _circleFollow.enabled = false;
        _seekingFollow.Paused = true;
        _isDead = true;
    }
}
