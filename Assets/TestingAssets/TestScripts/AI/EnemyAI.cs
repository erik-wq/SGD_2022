using Assets.TestingAssets;
using Assets.TestingAssets.TestScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour, IEnemy
{
    #region Serialized
    [SerializeField] protected Transform PlayerTransform;
    [SerializeField] protected PlayerController Player;

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
    [SerializeField] protected SpriteRenderer MainSprite;

    //Debug
    [SerializeField] protected SpriteRenderer Sword;
    #endregion

    #region Private
    protected Rigidbody2D _rigidBody;
    protected IFollow _followScript;
    protected float _lastAttackTime;
    protected float _executionStartedAt;
    protected bool _executingAttack = false;
    protected float _currentHP;
    protected float _knockbackStart;
    protected bool _knockbackCleared = true;
    protected float _maxOpacity = 1.0f;
    #endregion

    // Start is called before the first frame update
    protected void Start()
    {
        _followScript = GetComponent<IFollow>();
        _rigidBody = GetComponent<Rigidbody2D>();
        _currentHP = MaxHP;
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
        CheckAttack();
        ClearKnockback();
    }

    protected void ClearKnockback()
    {
        if(!_knockbackCleared)
        {
            if(Time.time > _knockbackStart + KnockbackLenght)
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
            Sword.enabled = true;
        }

        if(_executingAttack && (Time.time - _executionStartedAt) >= AttackDelay)
        {
            _executingAttack = false;
            Sword.enabled = false;
            _lastAttackTime = Time.time;
            if(CheckAttackRangeHit())
            {
                Player.TakeHit(Damage, _rigidBody.position, AttackKnockbackPower);
            }
        }
    }

    protected bool CheckAttackRange()
    {
        return Vector2.Distance(_rigidBody.position, PlayerTransform.position) <= AttackRangeDetection;
    }

    protected bool CheckAttackRangeHit()
    {
        return Vector2.Distance(_rigidBody.position, PlayerTransform.position) <= AttackRange;
    }

    protected void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag == "Player")
        {
            _followScript.SetTarget(PlayerTransform);
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
            UnityEngine.Object.Destroy(gameObject);
            return true;
        }
        else
        {
            AdjustOpacity();
            ApplyForce(force, origin);
            return false;
        }
    }

    private void AdjustOpacity()
    {
        float percentageHP = _currentHP / MaxHP;
        float opacity = _maxOpacity * percentageHP;
        MainSprite.color = new Color(MainSprite.color.r, MainSprite.color.g, MainSprite.color.b, opacity);
    }
}
