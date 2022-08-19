using Assets.Scripts.Utils;
using Assets.TestingAssets;
using Assets.TestingAssets.TestScripts;
using Assets.TestingAssets.TestScripts.AI;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShadowCircleFollow : MonoBehaviour, IFollow
{
    #region Public
    public Transform Target { get; set; }
    public bool Paused { get; set; }
    #endregion

    #region Serialized
    [SerializeField] private Seeker Seeker;
    [SerializeField] private float MovementSpeed = 5f;
    [SerializeField] private SpriteRenderer _mainSprite;
    [SerializeField] private Animator MainAnimator;

    [SerializeField] private float Damage = 20;
    [SerializeField] private float CrossingCooldown = 5f;
    [SerializeField] private float CrossingChance = 0.6f;

    [SerializeField] private float CrossingSpeed = 6;
    [SerializeField] private AnimationCurve CrossingCurve;
    [SerializeField] private float CrossingCurveTimeLength = 2;
    [SerializeField] private bool PredictiveCrossing = false;
    [SerializeField] private float CrossingChargingLength = 1;

    [SerializeField] private ContactFilter2D CollisionsFilter;
    [SerializeField] private string CollisionGraphMaskName = "Shadows";

    [SerializeField] private float CircleStepSize = 10;
    [SerializeField] private float AroundCircleReachTolerance = 0.5f;
    [SerializeField] private float CrossingReachTolerance = 0.1f;
    [SerializeField] private float CollisionRadius = 1f;
    [SerializeField] private float TakenDamageRecoveryDelay = 3f;

    [SerializeField] private Transform DebugTransform;
    [SerializeField] private Transform AttackAnimationTransform;
    #endregion

    #region Private
    private bool _circleClockwise = true;
    private float _possitionAtCircle = 0;
    private Vector2 _defaultZeroPoint = Vector2.zero;
    private Vector2 _nextTarget = Vector2.zero;
    private bool _isCrossing = false;
    private float _currentSpeed;
    private float _circleRadius;
    private Transform _target;
    private Path _path;
    private float _crossEndTime = 0;
    private float _lastCheck = 0;
    private float _crossStartTime = 0f;
    private Func<float> _getRotation;
    private Func<float, float> _setRotation;
    private float _animationRotationOffset = -90;
    private bool _isAnimationLocked = false;
    private bool _hitPlayer = false;
    private bool _hitHope = false;
    private float _takenDamageTime = 0;
    #endregion

    #region Public
    public float CircleRadius
    {
        get
        {
            return _circleRadius;
        }

        set
        {
            _circleRadius = value;
        }
    }
    #endregion
    private void Start()
    {
        InvokeRepeating("UpdatePath", 0f, 0.2f);
        _currentSpeed = MovementSpeed;
        Paused = true;
    }

    public void Init(Func<float> getRotatio, Func<float, float> setRotation)
    {
        this._getRotation = getRotatio;
        this._setRotation = setRotation;
    }

    private void UpdatePath()
    {
        if (_target != null)
        {
            Seeker.StartPath(transform.position, _target.position, OnPathComplete, GraphMask.FromGraphName(CollisionGraphMaskName));
        }
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            _path = p;
        }
    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void FixedUpdate()
    {
        if (Target == null || Paused || _isAnimationLocked)
        {
            return;
        }

        if (_isCrossing)
        {
            MoveToCrossTarget();
            CheckReachedCrossingTarget();
            AdjustFacingDirection();
        }
        else
        {
            AdjustFacingDirection();
            MoveToPointOnCircle();
            CheckFlyingAroundCircle();
            CheckCrossTransition();
        }
    }

    private void CheckCrossTransition()
    {
        if (!_isCrossing && Time.time > _crossEndTime + CrossingCooldown && Time.time > _lastCheck + 1)
        {
            if (UnityEngine.Random.Range(0, 1) < CrossingChance)
            {
                Cross();
            }
        }
    }

    private void MoveToCrossTarget()
    {
        var direction = GetDirectionToNextPoint();
        var crossSpeed = GetCrossingSpeed();
        var moveVector = direction * crossSpeed * Time.fixedDeltaTime;

        var nextDirection = (_nextTarget - ((Vector2)this.transform.position + moveVector)).normalized;

        if (nextDirection != direction)
        {
            this.transform.position = _nextTarget;
        }
        else
        {
            this.transform.position += new Vector3(moveVector.x, moveVector.y, 0);
        }
    }

    private float GetCrossingSpeed()
    {
        var percent = ((Time.time - _crossStartTime) / CrossingCurveTimeLength);
        if (percent > 1)
            percent = 1;
        return CrossingSpeed * CrossingCurve.Evaluate(percent);
    }

    private void MoveToPointOnCircle()
    {
        var moveVector = GetDirectionToNextPoint() * MovementSpeed * Time.fixedDeltaTime;
        this.transform.position += new Vector3(moveVector.x, moveVector.y, 0);
    }

    private void CheckFlyingAroundCircle()
    {
        var distance = Vector2.Distance((Vector2)this.transform.position, _nextTarget);
        if (Mathf.Abs(distance) <= AroundCircleReachTolerance || _nextTarget == Vector2.zero)
        {
            GenerateNextTargetOnCircle();
        }
    }

    private void AdjustFacingDirection()
    {
        if (DebugTransform != null)
            DebugTransform.transform.position = _nextTarget;

        var direction = GetDirectionToNextPoint();
        var angle = MathUtility.FullAngle(Vector2.up, direction);
        _setRotation(angle);
    }

    private void GenerateNextTargetOnCircle()
    {
        int maxItteration = Convert.ToInt32(360.0f / CircleStepSize);
        int i = 0;
        _possitionAtCircle = MathUtility.NormalizeAngle(_possitionAtCircle);
        while (true)
        {
            _possitionAtCircle += (_circleClockwise) ? CircleStepSize : -CircleStepSize;
            var nextDirection = MathUtility.RotateVector(_defaultZeroPoint, _possitionAtCircle);
            var nextPossition = (Vector2)Target.position + (nextDirection * CircleRadius);
            if (CheckPossitionForCollisions(nextPossition))
            {
                _nextTarget = nextPossition;
                return;
            }

            i++;
            if (i > maxItteration)
                break;
        }
    }

    private bool CheckPossitionForCollisions(Vector2 possition)
    {
        var collidedGameObjects = Physics.OverlapSphere(possition, CollisionRadius)
                                        .Where(a => a.gameObject.layer == (int)LayersNaming.LargeCollisionObjects)
                                        .Select(c => c.gameObject)
                                        .ToList();
        return collidedGameObjects.Count <= 0;
    }

    private Vector2 GetDirectionToTarget()
    {
        return ((Vector2)Target.position - (Vector2)this.transform.position).normalized;
    }

    private Vector2 GetDirectionToNextPoint()
    {
        return (_nextTarget - (Vector2)this.transform.position).normalized;
    }

    private Vector2 GetDirectionFromTarget()
    {
        return ((Vector2)this.transform.position - (Vector2)Target.position).normalized;
    }

    public void SetTarget(Transform transform)
    {
        this.Target = transform;
    }

    private bool Cross()
    {
        if (_isCrossing)
            return false;

        _hitPlayer = false;
        _hitHope = false;

        _isAnimationLocked = false;
        _isCrossing = true;
        _crossStartTime = Time.time;
        _nextTarget = (Vector2)Target.position + GetDirectionToTarget() * _circleRadius;
        PlayAttackAnimation();

        return true;
    }

    private bool CreateCrossTransition()
    {
        GetComponentInChildren<SpriteRenderer>().enabled = false;
        return false;
    }

    public void InitCircle()
    {
        _defaultZeroPoint = GetDirectionFromTarget();
        _possitionAtCircle = 0;
        _isCrossing = false;
        GenerateNextTargetOnCircle();
    }

    private void CheckReachedCrossingTarget()
    {
        var distance = Vector2.Distance((Vector2)this.transform.position, (Vector2)_nextTarget);
        if (distance <= CrossingReachTolerance)
        {
            _defaultZeroPoint = GetDirectionFromTarget();
            _possitionAtCircle = 0;
            _isCrossing = false;
            _crossEndTime = Time.time;
            GenerateNextTargetOnCircle();
        }
    }

    public void TakeDamage(float damage)
    {
        if (_isCrossing)
        {
            var toTarget = GetDirectionToTarget();
            var toPoint = GetDirectionToNextPoint();
            _takenDamageTime = Time.time;

            if (Vector2.Dot(toTarget, toPoint) > 0.25f)
            {
                toPoint = toPoint * -1;
                toPoint = MathUtility.RotateVector(toPoint, 15);
                _nextTarget = toPoint;
            }
        }
    }

    public void OnAttackEnds()
    {
        _mainSprite.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Hope")
        {
            if (Time.time < _takenDamageTime + TakenDamageRecoveryDelay)
                return;

            if (collision.gameObject.tag == "Player" && _hitPlayer)
            {
                return;
            }
            else
            {
                _hitPlayer = true;
            }

            if (collision.gameObject.tag == "Hope" && _hitHope)
            {
                return;
            }
            else
            {
                _hitHope = true;
            }

            IEnemy iEnemy = collision.gameObject.GetComponent<IEnemy>();
            if (iEnemy == null)
            {
                iEnemy = collision.gameObject.GetComponentInParent<IEnemy>();
            }

            if (iEnemy == null)
            {
                iEnemy = collision.transform.parent.GetComponentInChildren<IEnemy>();
            }

            iEnemy.TakeDamage(Damage);
        }
    }

    private void PlayAttackAnimation()
    {
        MainAnimator.Play("GhostAttack");
    }
}
