using Assets.TestingAssets.TestScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    #region Serialized
    [SerializeField] protected Transform PlayerTransform;
    [SerializeField] protected PlayerController Player;

    [SerializeField] protected int HP = 10;
    [SerializeField] protected int Damage = 5;
    [SerializeField] protected float KnockbackModifier = 1f;
    [SerializeField] protected float AttackKnockbackPower = 1f;
    [SerializeField] protected float AttackSpeed = 1f;
    [SerializeField] protected float AttackRadius = 360; //Not yet implemented
    [SerializeField] protected float AttackRangeDetection = 2.0f;
    [SerializeField] protected float AttackRange = 3.0f;
    [SerializeField] protected float AttackDelay = 1.3f;

    //Debug
    [SerializeField] protected SpriteRenderer Sword;
    #endregion

    #region Private
    protected Rigidbody2D _rigidBody;
    protected IFollow _followScript;
    protected float _lastAttackTime;
    protected float _executionStartedAt;
    protected bool _executingAttack = false;
    #endregion

    // Start is called before the first frame update
    protected void Start()
    {
        _followScript = GetComponent<IFollow>();
        _rigidBody = GetComponent<Rigidbody2D>();
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
}
