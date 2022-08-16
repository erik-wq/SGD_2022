using Assets.TestingAssets;
using Assets.TestingAssets.TestScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.Rendering.Universal;
using Assets.Scripts.Utils;
using Assets.Scripts;

public class EnemyAI : MonoBehaviour, IEnemy
{
    #region Serialized
    [SerializeField] protected Transform PlayerTransform;
    [SerializeField] protected Transform HopeTransform;
    [SerializeField] protected PlayerController Player;
    [SerializeField] protected HopeAI HopeAIScript;

    [SerializeField] protected int MaxHP = 100;
    [SerializeField] protected int Damage = 5;
    //[SerializeField] protected float KnockbackModifier = 1f;
    [SerializeField] protected float AttackKnockbackPower = 1f;
    [SerializeField] protected float AttackSpeed = 1f;
    [SerializeField] protected float AttackRadius = 360; //Not yet implemented
    [SerializeField] protected float AttackRangeDetection = 2.0f;
    [SerializeField] protected float AttackRange = 3.0f;
    [SerializeField] protected float AttackDelay = 1.3f;
    [SerializeField] protected float KnockbackLenght = 1.3f;
    [SerializeField] protected float MinimalAlpha = 0.3f;
    [SerializeField] protected SpriteRenderer MainSprite;

    [SerializeField] protected GameObject SpikesPrefab;
    [SerializeField] protected int SpikesCount = 12;
    [SerializeField] protected float SpikesSpeed = 25;
    [SerializeField] protected int SpikesDamage = 15;
    [SerializeField] protected int SpikesWaves = 2;
    [SerializeField] protected float SpikesWaveDelay = 0.5f;
    [SerializeField] protected float SpikesCooldown = 10;
    [SerializeField] protected float SpikesRange = 100;
    [SerializeField] protected float SpikeSpawnRange = 4;

    #endregion

    #region Private
    protected Rigidbody2D _rigidBody;
    protected BasicFollow _followScript;
    protected float _lastAttackTime;
    protected float _executionStartedAt;
    protected bool _executingAttack = false;
    protected float _currentHP;
    protected float _knockbackStart;
    protected bool _knockbackCleared = true;
    protected float _maxOpacity = 1.0f;
    protected bool _circleClockwise = false;
    protected float _lastAoeFiret = 0;
    protected Animator _animator;
    protected float _rotationOffset = -90;
    private bool _isDead = false;
    protected EnemyControllerSingleton _enemyControl = EnemyControllerSingleton.GetInstance();

    protected bool _hasAggro = false;
    #endregion
    public GameObject effect;
    // Start is called before the first frame update
    protected void Start()
    {
        _followScript = GetComponent<BasicFollow>();
        _followScript.BindOnMove(FaceDirection);
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _currentHP = MaxHP;

        if (HopeTransform == null || PlayerTransform == null || Player == null || HopeAIScript == null)
        {
            LoadBasics();
        }
    }

    // Update is called once per frame
    protected void Update()
    {

    }

    /// <summary>
    /// Checks whether to attack
    /// </summary>
    protected void FixedUpdate()
    {
        if (_hasAggro && !_isDead)
        {
            CheckAttack();
            ClearKnockback();
            CheckAoe();
        }
    }

    private void LoadBasics()
    {
        HopeAIScript = Global.Instance.HopeScript;
        HopeTransform = Global.Instance.HopeTransform;
        Player = Global.Instance.PlayerScript;
        PlayerTransform = Global.Instance.PlayerTransform;
        _followScript = GetComponent<BasicFollow>();
    }

    protected void CheckAoe()
    {
        if (Time.time > _lastAoeFiret + SpikesCooldown)
        {
            if (_enemyControl.AskToFire())
            {
                FireAoeRange();
            }
        }
    }

    protected void ClearKnockback()
    {
        if (!_knockbackCleared)
        {
            if (Time.time > _knockbackStart + KnockbackLenght)
            {
                _rigidBody.velocity = Vector3.zero;
                _rigidBody.angularVelocity = 0;
                _knockbackCleared = true;
                _followScript.Paused = false;
            }
        }
    }

    protected void CheckAttack()
    {
        if (CheckAttackRange() && (Time.time - _lastAttackTime) > AttackSpeed && !_executingAttack)
        {
            _executingAttack = true;
            _executionStartedAt = Time.time;
            _animator.Play("SpikeMeeleAttack");
        }
    }

    public void DoMeleeDamageFromAnimation()
    {
        if (_executingAttack)
        {
            _executingAttack = false;
            _lastAttackTime = Time.time;
            if (CheckAttackRangeHit())
            {
                Player.TakeDamage(Damage, AttackKnockbackPower, _rigidBody.position);
            }

            if (CheckAttackRangeHitHope())
            {
                HopeAIScript.TakeDamage(Damage);
            }
        }
    }

    protected bool CheckAttackRange()
    {
        return Vector2.Distance(_rigidBody.position, PlayerTransform.position) <= AttackRangeDetection;
    }

    protected bool CheckAttackRangeHitHope()
    {
        return Vector2.Distance(_rigidBody.position, HopeTransform.position) <= AttackRangeDetection;
    }

    protected bool CheckAttackRangeHit()
    {
        return Vector2.Distance(_rigidBody.position, PlayerTransform.position) <= AttackRange;
    }

    protected void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player" || col.gameObject.tag == "Hope")
        {
            if (PlayerTransform == null)
                LoadBasics();

            _followScript.SetTarget(PlayerTransform);
            _hasAggro = true;
            _enemyControl.Register(this);
        }
    }

    protected void ApplyForce(float force, Vector2 origin)
    {
        _knockbackStart = Time.time;
        _knockbackCleared = false;
        _followScript.Paused = true;
        var direction = ((Vector2)this.transform.position - origin).normalized;
        this._rigidBody.AddForce(direction * force, ForceMode2D.Force);
    }

    public bool TakeDamage(float damage, float force, Vector2 origin)
    {
        _currentHP -= damage;
        if (_currentHP < 0)
        {
            Killed();
            return true;
        }
        else
        {
            //AdjustOpacity();
            ApplyForce(force, origin);
            _animator.Play("SpikeTakeDamage");
            return false;
        }
    }

    private void FaceDirection(Vector2 direction)
    {
        //var angle = MathUtility.FullAngle(Vector2.up, direction);
        //this.transform.rotation = Quaternion.Euler(0, 0, angle + _rotationOffset);
    }

    public bool TakeDamage(float damage)
    {
        _currentHP -= damage;
        if (_currentHP < 0)
        {
            Killed();
            return true;
        }
        else
        {
            //AdjustOpacity();
            _animator.Play("SpikeTakeDamage");
            return false;
        }
    }

    private void FireAoeRange()
    {
        _animator.Play("SpikeRangeAttack");
    }

    public void FireAoeFromAnimation()
    {
        if (_isDead)
            return;

        var step = 360.0f / SpikesCount;
        var offSet = UnityEngine.Random.Range(0, step);

        for (int i = 0; i < SpikesCount; i++)
        {
            var direction = MathUtility.RotateVector(Vector2.up, (step * i) + offSet).normalized;
            var possition = (Vector2)this.transform.position + (direction * SpikeSpawnRange);
            var newSpike = Instantiate(SpikesPrefab, possition, Quaternion.identity);
            var projectileLogic = newSpike.AddComponent<ProjectileLogic>();
            projectileLogic.MaxRange = SpikesRange;
            projectileLogic.ProjectileDamage = SpikesDamage;
            projectileLogic.ProjectileSpeed = SpikesSpeed;
            projectileLogic.SetDirection(direction);
        }

        _lastAoeFiret = Time.time;
    }

    private void AdjustOpacity()
    {
        float percentageHP = _currentHP / MaxHP;
        float opacity = _maxOpacity * percentageHP;
        if (opacity < MinimalAlpha)
            opacity = MinimalAlpha;
        MainSprite.color = new Color(MainSprite.color.r, MainSprite.color.g, MainSprite.color.b, opacity);
    }
    private void Killed()
    {
        _rigidBody.velocity = Vector2.zero;
        MainSprite.enabled = false;
        _isDead = true;
        effect.SetActive(true);
        Collider2D[] cols = GetComponents<Collider2D>();
        foreach (Collider2D x in cols)
        {
            x.enabled = false;
        }

        GetComponent<Seeker>().enabled = false;
        GetComponent<DynamicGridObstacle>().enabled = false;
        //GetComponent<ShadowCaster2D>().enabled = false;
        GetComponent<BasicFollow>().enabled = false;
        _enemyControl.Unregister(this);
        this.enabled = false;
    }
}