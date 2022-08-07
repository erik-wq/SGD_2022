using Assets.Scripts.Utils;
using Assets.TestingAssets.TestScripts;
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

    public float CircleRadius { get { return _circleRadius; } }
    #endregion

    #region Serialized
    [SerializeField] private Seeker Seeker;
    [SerializeField] private float MovementSpeed = 5f;
    [SerializeField] private float CircleRadiusMean = 8f;
    [SerializeField] private float CircleRadiusStd = 2f;

    [SerializeField] private float CrossingCooldown = 5f;
    [SerializeField] private float CrossingChance = 0.6f;
    [SerializeField] private float CrossingSpeed = 6;
    [SerializeField] private AnimationCurve CrossingCurve;
    [SerializeField] private float CrossingCurveTimeLength = 2;
    [SerializeField] private bool PredictiveCrossing = false;
    [SerializeField] private float CrossingCharingLength = 1;

    [SerializeField] private ContactFilter2D CollisionsFilter;
    [SerializeField] private string CollisionGraphMaskName = "Shadows";

    [SerializeField] private float CircleStepSize = 10;
    [SerializeField] private float AroundCircleReachTolerance = 0.5f;
    [SerializeField] private float CrossingReachTolerance = 0.1f;
    [SerializeField] private float CollisionRadius = 1f;
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
    private Func<float> _getRotation;
    private Func<float, float> _setRotation;
    #endregion

    #region Public
    public float GetCircleRadius
    {
        get
        {
            return _circleRadius;
        }
    }
    #endregion
    // Start is called before the first frame update
    private void Start()
    {
        InvokeRepeating("UpdatePath", 0f, 0.2f);
        _currentSpeed = MovementSpeed;
        Paused = true;
        _circleRadius = MathUtility.NormalRNG(CircleRadiusMean, CircleRadiusStd);
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
        if (Target == null || Paused)
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
            MoveToPointOnCircle();
            CheckFlyingAroundCircle();
            CheckCrossTransition();
            AdjustFacingDirection();
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
        var moveVector = GetDirectionToNextPoint() * MovementSpeed * Time.fixedDeltaTime;
        this.transform.position += new Vector3(moveVector.x, moveVector.y, 0);
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
        
        _isCrossing = true;
        _nextTarget = (Vector2)Target.position + GetDirectionToTarget() * _circleRadius;
        return true;
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
        if(_isCrossing)
        {
            var toTarget = GetDirectionToTarget();
            var toPoint = GetDirectionToNextPoint();

            if(Vector2.Dot(toTarget, toPoint) > 0.25f)
            {
                toPoint = toPoint * -1;
                toPoint = MathUtility.RotateVector(toPoint, 15);
                _nextTarget = toPoint;
            }
        }
    }
}
