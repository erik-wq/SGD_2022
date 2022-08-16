using Assets.Scripts;
using Assets.Scripts.Utils;
using Assets.TestingAssets;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IEnemy
{
    #region Serialized
    [SerializeField] private float MovementSpeedForward = 6f;
    [SerializeField] private float MovementSpeedForwardSideway = 8f;
    [SerializeField] private float MovementSpeedBackwards = 4f;
    [SerializeField] private float MovementSpeedBackwardsSideway = 3f;
    [SerializeField] private float MaxHP = 100;
    [SerializeField] private Image HealthBar;
    [SerializeField] private float HPRegen = 1.5f;

    [SerializeField] private float CollisionOffset = 0.05f;

    [SerializeField] private ContactFilter2D CollisionsFilter;
    [SerializeField] private AstarPath Pathfinder;
    [SerializeField] private BasicFollow HopesFollow;
    [SerializeField] private HopeAI HopeAI;
    [SerializeField] private Camera MainCamera;
    [SerializeField] private CameraManagement CameraManagement;

    //Sword
    [SerializeField] private SpriteRenderer MainSprite;
    [SerializeField] private float SwordSlashZonesCount = 6;
    [SerializeField] private float SwordAnimationLength = 2;
    [SerializeField] private float SwordAttackCooldown = 3;
    [SerializeField] private float SwordAnimationDistance = 4;
    [SerializeField] private float SwordSlashRadius = 4;
    [SerializeField] private ContactFilter2D EnemyFilter;
    [SerializeField] private float SwordDamage = 4;
    [SerializeField] private float SwordForce = 500;

    [SerializeField] private float AttackMovementspeedForward = 10;
    [SerializeField] private float AttackMovementspeedBackwards = 10;

    //Sprint
    [SerializeField] private AnimationCurve SprintCurve;
    [SerializeField] private float CurveLength = 3;
    [SerializeField] private float SprintSpeed = 5;
    [SerializeField] private float MaxEnergy = 100;
    [SerializeField] private float SprintEnergyCost = 10;
    [SerializeField] private float MinimalEnergyToStartSprint = 15;
    [SerializeField] private float EnergyRechargeRateWhileStaying = 25;
    [SerializeField] private float EnergyRechargeRateWhileRunning = 15;
    [SerializeField] private float EnergyRechardDelayWhileStaying = 0.4f;
    [SerializeField] private float EnergyRechardDelayWhileRunning = 0.8f;
    #endregion

    #region Private
    private Vector2 _movementInput = Vector2.zero;
    private Rigidbody2D _rigidBody2D;
    private bool _isSwordSlashInProgress = false;
    private float _lastSlashTime = 0;
    private float _currentEnergy = 0;

    private List<Collider2D> _swordSlashZones = new List<Collider2D>();
    private Animator _animator;
    private float _currentHP;

    //Sprint
    private float _sprintStart;
    private bool _sprintInProgress = false;
    private float _sprintRegargeCounter = 0;
    private bool _isHeavyAttacking = false;
    private bool _isAttackDirectionRight = false;

    private Vector2 _dashDirection;
    private bool dashing = false;
    private bool canDash = true;
    private Vector3 effectOfset;
    #endregion

    #region Public
    public bool IsMovementLocked { get; set; }
    public bool ActionLocked { get; set; }

    public float dashTime = 0.25f;
    public float dashSpeedModifier = 5;
    public float dashCooldown = 1.25f;
    public ParticleSystem dashEffect;
    #endregion

    private void Awake()
    {
        effectOfset = dashEffect.transform.position - transform.position;
        EnemyControllerSingleton.GetInstance().Init();
    }

    // Start is called before the first frame update
    private void Start()
    {
        _currentHP = MaxHP;
        _rigidBody2D = GetComponent<Rigidbody2D>();
        IsMovementLocked = false;
        _animator = GetComponentInChildren<Animator>();

        Global.Instance.PlayerTransform = this.transform;
        Global.Instance.PlayerScript = this;
        GenerateSlashZones();
    }

    private void FixedUpdate()
    {
        if (dashing)
        {
            //_rigidBody2D.velocity = _dashDirection * dashSpeedModifier * MovementSpeedForward * Time.fixedDeltaTime;
            _rigidBody2D.MovePosition(_rigidBody2D.position + _dashDirection * MovementSpeedForward * dashSpeedModifier *Time.fixedDeltaTime);
            return;
        }
        CheckSwordAnimation();
        HandleDash();
        //HandleSprint();
        if (_movementInput != Vector2.zero && !IsMovementLocked)
        {
            Move(_movementInput);
            AdjustFlip(_movementInput);
            _animator.SetBool("IsRunning", true);
        }
        else
        {
            CameraManagement.SetMoving(false);
            _animator.SetBool("IsRunning", false);
        }

        RunHpRegen();
    }
    private void LateUpdate()
    {
        if (dashing)
        {
            dashEffect.transform.position = transform.position + effectOfset;
        }
    }

    private void RunHpRegen()
    {
        _currentHP += Time.fixedDeltaTime * HPRegen;
        if (_currentHP > MaxHP)
            _currentHP = MaxHP;
        AdjustHealthBar();
    }

    public void OnAttackFinished()
    {
        _isHeavyAttacking = false;
    }

    private void GenerateSlashZones()
    {
        var step = 360.0f / SwordSlashZonesCount;
        for (int i = 0; i < SwordSlashZonesCount; i++)
        {
            var polygon = this.gameObject.AddComponent<PolygonCollider2D>();
            polygon.isTrigger = true;
            var points = new List<Vector2>();
            points.Add(Vector2.zero);
            points.Add(MathUtility.RotateVector(new Vector2(0, SwordSlashRadius), i * step));
            points.Add(MathUtility.RotateVector(new Vector2(0, SwordSlashRadius), (i * step) + (step / 6)));
            points.Add(MathUtility.RotateVector(new Vector2(0, SwordSlashRadius), (i * step) + ((step / 6) * 2)));
            points.Add(MathUtility.RotateVector(new Vector2(0, SwordSlashRadius), (i * step) + ((step / 6) * 3)));
            points.Add(MathUtility.RotateVector(new Vector2(0, SwordSlashRadius), (i * step) + ((step / 6) * 4)));
            points.Add(MathUtility.RotateVector(new Vector2(0, SwordSlashRadius), (i * step) + ((step / 6) * 5)));
            points.Add(MathUtility.RotateVector(new Vector2(0, SwordSlashRadius), (i * step) + ((step / 6) * 6)));
            polygon.points = points.ToArray();
            _swordSlashZones.Add(polygon);
        }

        TurnOffSlashZones();
    }

    private void AdjustFlip(Vector2 direction)
    {
        if (_isHeavyAttacking)
            return;

        if (direction.x >= 0)
        {
            MainSprite.flipX = false;
        }
        else
        {
            MainSprite.flipX = true;
        }
    }

    private void HandleSprint()
    {
        if (_sprintInProgress)
        {
            if (!Input.GetKey(KeyCode.Space) || _movementInput == Vector2.zero)
            {
                ExitSprint();
            }
            else
            {
                SprintConsumeEnergy();
            }
        }
        else
        {
            if (_currentEnergy < MaxEnergy)
            {
                RegainEnergy();
            }
        }
    }

    private void TurnOffSlashZones()
    {
        foreach (var item in _swordSlashZones)
        {
            if (item != null)
            {
                item.enabled = false;
            }
        }
    }

    private void TurnOnSlashZones()
    {
        foreach (var item in _swordSlashZones)
        {
            if (item != null)
            {
                item.enabled = true;
            }
        }
    }

    private void SprintConsumeEnergy()
    {
        _currentEnergy -= SprintEnergyCost * Time.fixedDeltaTime;
        if (_currentEnergy < 0)
        {
            _currentEnergy = 0;
            _sprintInProgress = false;
            _animator.SetBool("IsSprinting", false);
        }
    }

    private void RegainEnergy()
    {
        var staying = _movementInput == Vector2.zero;
        _sprintRegargeCounter += Time.fixedDeltaTime;
        if (staying && _sprintRegargeCounter > EnergyRechardDelayWhileStaying)
        {
            _currentEnergy += EnergyRechargeRateWhileStaying * Time.fixedDeltaTime;
            if (_currentEnergy > MaxEnergy)
                _currentEnergy = MaxEnergy;
        }
        else if (_sprintRegargeCounter > EnergyRechardDelayWhileRunning)
        {
            _currentEnergy += EnergyRechargeRateWhileRunning * Time.fixedDeltaTime;
            if (_currentEnergy > MaxEnergy)
                _currentEnergy = MaxEnergy;
        }
    }

    private void ExitSprint()
    {
        _sprintInProgress = false;
        _animator.SetBool("IsSprinting", false);
        _sprintRegargeCounter = 0;
    }

    private void AdjustHealthBar()
    {
        HealthBar.fillAmount = _currentHP / MaxHP;
    }

    private void CheckSwordAnimation()
    {
        if (Time.time > _lastSlashTime + SwordAnimationLength)
        {
            _isSwordSlashInProgress = false;
        }
    }

    private void StartSprint()
    {
        if (_currentEnergy > MinimalEnergyToStartSprint)
        {
            _sprintStart = Time.time;
            _sprintInProgress = true;
            _animator.SetBool("IsSprinting", true);
            _sprintRegargeCounter = 0;
        }
    }

    private Vector2 GetFaceDirection()
    {
        return CustomUtilities.GetMouseDirection(MainCamera, this.transform);
    }

    private float GetMovementSpeed(Vector2 input)
    {
        float dotProduct = Vector2.Dot(input, GetFacingDirectionRounded());
        if (dotProduct == -1)
        {
            return MovementSpeedBackwards;
        }
        else if (dotProduct < -1)
        {
            return MovementSpeedBackwardsSideway;
        }
        else
        {
            if (input.x == 0 || input.y == 0)
            {
                return MovementSpeedForward;
            }
            else
            {
                return MovementSpeedForwardSideway;
            }
        }
    }

    private float GetHeavyAttackMovementSpeed(Vector2 input)
    {
        if (_isAttackDirectionRight && input.x > 0)
        {
            return AttackMovementspeedForward;
        }
        else if (!_isAttackDirectionRight && input.x <= 0)
        {
            return AttackMovementspeedForward;
        }
        return AttackMovementspeedBackwards;
    }

    public Vector2 GetFacingDirectionRounded()
    {
        var direction = GetFaceDirection();
        return new Vector2(Mathf.RoundToInt(direction.x), Mathf.RoundToInt(direction.y));
    }

    public float GetMovementSpeedWithSprint(Vector2 input)
    {
        var basicSpeed = GetMovementSpeed(input);
        var pointAtSprint = (Time.time - _sprintStart) / CurveLength;
        var value = SprintCurve.Evaluate(pointAtSprint);
        return basicSpeed + value * SprintSpeed;
    }

    private void DamageEnemies(Vector2 direction)
    {
        if (_swordSlashZones.Count == 0)
            return;

        TurnOnSlashZones();
        var angle = MathUtility.FullAngle(Vector2.up, direction);
        var step = 360.0f / SwordSlashZonesCount;

        var zone = Convert.ToInt32(Math.Floor(angle / step));

        var enemies = new List<Collider2D>();
        _swordSlashZones[zone].OverlapCollider(EnemyFilter, enemies);

        foreach (var item in enemies)
        {
            if (item.gameObject.tag == "EnemyHitCollider")
            {
                IEnemy iEnemy = item.gameObject.GetComponent<IEnemy>();
                if (iEnemy == null)
                {
                    iEnemy = item.gameObject.GetComponentInParent<IEnemy>();
                }

                if(iEnemy == null)
                {
                    iEnemy = item.transform.parent.GetComponentInChildren<IEnemy>();
                }

                iEnemy.TakeDamage(SwordDamage, SwordForce, this.transform.position);
            }
        }
        TurnOffSlashZones();
    }
    private void HandleDash()
    {
        if(_movementInput == Vector2.zero)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (canDash)
            {
                dashEffect.transform.position = transform.position + effectOfset;
                _dashDirection = _movementInput;
                canDash = false;

                dashing = true;
                StartCoroutine(Dash());
            }
        }
    }

    private IEnumerator Dash()
    {
        ParticleSystemRenderer renderer;
        dashEffect.TryGetComponent<ParticleSystemRenderer>(out renderer);
        if(renderer != null)
        {
            renderer.flip = DashVector();
        }
        dashEffect.Play();
        
        yield return new WaitForSeconds(dashTime);
        dashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    private Vector3 DashVector()
    {
        if(_dashDirection.x > 0)
        {
            return Vector3.zero;
        }
        if(_dashDirection.x == 0)
        {
            return Vector3.zero;
        }
        return new Vector3(1,0,0);
    }

    #region Inputs registering
    private void OnMove(InputValue input)
    {
        _movementInput = input.Get<Vector2>();
    }
    #endregion

    #region Inputs handeling
    private void Move(Vector2 input)
    {
        if (input == Vector2.zero)
            return;

        float movementSpeed = 0;

        if (!_isHeavyAttacking)
        {
            if (_sprintInProgress)
            {
                movementSpeed = GetMovementSpeedWithSprint(input);
            }
            else
            {
                movementSpeed = GetMovementSpeed(input);
            }
        }
        else
        {
            movementSpeed = GetHeavyAttackMovementSpeed(input);
        }

        CameraManagement.SetPlayersSpeed(movementSpeed);

        //if (TryMove(input, movementSpeed).Count == 0)
        //{
        _rigidBody2D.MovePosition(_rigidBody2D.position + input * movementSpeed * Time.fixedDeltaTime);
        //}
    }

    private List<RaycastHit2D> TryMove(Vector2 direction, float movementSpeed)
    {
        List<RaycastHit2D> outCollisions = new List<RaycastHit2D>();

        Physics2D.queriesHitTriggers = false;
        _rigidBody2D.Cast(direction,
                    CollisionsFilter,
                    outCollisions,
                    GetMoveDistance(movementSpeed) + CollisionOffset
                  );

        Physics2D.queriesHitTriggers = true;
        return SoftUnstuck(direction, outCollisions);
    }

    private float GetMoveDistance(float movementSpeed)
    {
        return movementSpeed * Time.fixedDeltaTime;
    }

    private List<RaycastHit2D> SoftUnstuck(Vector2 direction, List<RaycastHit2D> collisions)
    {
        List<RaycastHit2D> relevantCollisions = new List<RaycastHit2D>();
        foreach (var item in collisions)
        {
            var toObject = ((Vector2)item.transform.position - (Vector2)_rigidBody2D.transform.position).normalized;
            if (Vector2.Dot(toObject, direction) > 0)
            {
                relevantCollisions.Add(item);
            }
        }

        return relevantCollisions;
    }

    private void Die()
    {

    }

    private void AdjustAttackDirection()
    {
        var facingDirection = GetFaceDirection();
        if (facingDirection.x > 0)
        {
            _isAttackDirectionRight = true;
            MainSprite.flipX = false;
        }
        else
        {
            _isAttackDirectionRight = false;
            MainSprite.flipX = true;
        }
    }

    private void OnFire()
    {
        if (!_isSwordSlashInProgress && Time.time > _lastSlashTime + SwordAttackCooldown && !ActionLocked)
        {
            _lastSlashTime = Time.time;
            _animator.Play("Player_Attack");
            _isHeavyAttacking = true;
            AdjustAttackDirection();
        }

        HopeAI.MouseClick();
    }

    public void DoDamageFromAnimation()
    {
        var direction = GetFaceDirection();
        DamageEnemies(direction);
    }

    private void OnSprint()
    {
        if (!ActionLocked)
            StartSprint();
    }

    private void OnFireLaser()
    {
        if (!ActionLocked)
            HopeAI.OnFireLaser();
    }

    private void OnThrow()
    {
        if (!ActionLocked)
            HopeAI.OnThrow();
    }

    private void OnExplode()
    {
        if (!ActionLocked)
            HopeAI.OnExplode();
    }

    public bool TakeDamage(float damage, float force, Vector2 origin)
    {
        _currentHP -= damage;
        _animator.Play("PlayerTakeDamage");
        if (_currentHP <= 0)
            Die();
        AdjustHealthBar();
        return false;
    }

    public bool TakeDamage(float damage)
    {
        return this.TakeDamage(damage, 0, Vector2.zero);
    }
    #endregion
}
