using Assets.Scripts.Utils;
using Assets.TestingAssets;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region Serialized
    [SerializeField] private float MovementSpeedForward = 6f;
    [SerializeField] private float MovementSpeedForwardSideway = 8f;
    [SerializeField] private float MovementSpeedBackwards = 4f;
    [SerializeField] private float MovementSpeedBackwardsSideway = 3f;

    [SerializeField] private float CollisionOffset = 0.05f;
    [SerializeField] private ContactFilter2D CollisionsFilter;
    [SerializeField] private AstarPath Pathfinder;
    [SerializeField] private BasicFollow HopesFollow;
    [SerializeField] private HopeAI HopeAI;
    [SerializeField] private Transform MouseIndicatorTransform;
    [SerializeField] private Camera MainCamera;
    [SerializeField] private CameraManagement CameraManagement;

    [SerializeField] private Transform debugCircle;

    [SerializeField] private DebugControl DebugScreenControl;

    //Sword
    [SerializeField] private SpriteRenderer SwordIndicator;
    [SerializeField] private SpriteRenderer MainSprite;
    [SerializeField] private Transform SwordIndicatorTransform;
    [SerializeField] private float SwordSlashZonesCount = 6;
    [SerializeField] private float SwordAnimationLength = 2;
    [SerializeField] private float SwordAttackCooldown = 3;
    [SerializeField] private float SwordAnimationDistance = 4;
    [SerializeField] private float SwordSlashRadius = 4;
    [SerializeField] private ContactFilter2D EnemyFilter;
    [SerializeField] private float SwordDamage = 4;
    [SerializeField] private float SwordForce = 500;


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
    private float _mouseIndicatorOffset = 2;
    private bool _isSwordSlashInProgress = false;
    private float _lastSlashTime = 0;
    private float _currentEnergy = 0;

    private List<Collider2D> _swordSlashZones = new List<Collider2D>();
    private Animator _animator;

    //Sprint
    private float _sprintStart;
    private bool _sprintInProgress = false;
    private float _sprintRegargeCounter = 0;
    private Vector2 _lastStateMovement = Vector2.zero;
    #endregion

    #region Public
    public bool IsMovementLocked { get; set; }
    public bool ActionLocked { get; set; }
    #endregion

    // Start is called before the first frame update
    private void Start()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
        IsMovementLocked = false;
        _animator = GetComponentInChildren<Animator>();
        GenerateSlashZones();
    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void FixedUpdate()
    {
        CheckSwordAnimation();
        HandleSprint();
        if (_movementInput != Vector2.zero && !IsMovementLocked)
        {
            Move(_movementInput);
            AdjustFlip(_movementInput);
            _animator.SetBool("IsRunning", true);
        }
        else
        {
            CameraManagement.SetMoving(false);
            DebugScreenControl.SetHeroSpeed(0);
            _animator.SetBool("IsRunning", false);
        }

        MoveIndicator();
        DebugScreenControl.SetEnergy(_currentEnergy);
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
            points.Add(MathUtility.RotateVector(new Vector2(0, 1), i * step));
            points.Add(MathUtility.RotateVector(new Vector2(0, 1), (i * step) + (step / 3)));
            points.Add(MathUtility.RotateVector(new Vector2(0, 1), (i * step) + ((step / 3) * 2)));
            points.Add(MathUtility.RotateVector(new Vector2(0, 1), (i * step) + step));
            polygon.points = points.ToArray();
            _swordSlashZones.Add(polygon);
        }

        TurnOffSlashZones();
    }

    private void AdjustFlip(Vector2 direction)
    {
        if(direction.x >= 0)
        {
            //transform.rotation = Quaternion.Euler(0, 0, 0);
            MainSprite.flipX = false;
        }
        else
        {
            MainSprite.flipX = true;
            //transform.rotation = Quaternion.Euler(0, 180, 0);
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

    private void CheckSwordAnimation()
    {
        if (Time.time > _lastSlashTime + SwordAnimationLength)
        {
            SwordIndicator.enabled = false;
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

    private void MoveIndicator()
    {
        Vector2 newPossition = (Vector2)this.transform.position + (_mouseIndicatorOffset * GetFaceDirection());
        MouseIndicatorTransform.position = new Vector3(newPossition.x, newPossition.y, MouseIndicatorTransform.position.z);
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

                iEnemy.TakeDamage(SwordDamage, SwordForce, this.transform.position);
            }
        }
        TurnOffSlashZones();
    }

    #region Public Functions
    public void TakeHit(int damage, Vector2 position, float attackKnockbackPower)
    {
        Vector2 normal = (_rigidBody2D.position - position).normalized;
        _rigidBody2D.AddForce(normal * attackKnockbackPower);
    }
    #endregion

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

        if (_sprintInProgress)
        {
            movementSpeed = GetMovementSpeedWithSprint(input);
        }
        else
        {
            movementSpeed = GetMovementSpeed(input);
        }

        CameraManagement.SetPlayersSpeed(movementSpeed);
        DebugScreenControl.SetHeroSpeed(movementSpeed);
        _rigidBody2D.MovePosition(_rigidBody2D.position + input * movementSpeed * Time.fixedDeltaTime);
    }

    private void OnFire()
    {
        if (!_isSwordSlashInProgress && Time.time > _lastSlashTime + SwordAttackCooldown && !ActionLocked)
        {
            var direction = GetFaceDirection();
            SwordIndicator.enabled = true;
            _lastSlashTime = Time.time;
            SwordIndicator.transform.localPosition = direction * SwordAnimationDistance;
            DamageEnemies(direction);
        }

        HopeAI.MouseClick();
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
    #endregion
}
