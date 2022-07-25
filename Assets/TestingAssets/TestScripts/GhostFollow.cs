using Assets.Scripts.Utils;
using Assets.TestingAssets.TestScripts;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GhostFollow : MonoBehaviour, IFollow
{
    #region Public
    [SerializeField] public Transform Target { get; set; }
    public bool Paused { get; set; }
    #endregion

    #region Serialized
    [SerializeField] private float MovementSpeed = 5f;
    //[SerializeField] private float NextWaypointDistance = 0.5f;
    [SerializeField] private float CircleRadius = 8f;
    [SerializeField] private ContactFilter2D CollisionsFilter;
    //[SerializeField] private string CollisionGraphMaskName = "Default";
    //[SerializeField] private float AttackFrequency = 4.0f;
    [SerializeField] private float CircleStepSize = 10;
    [SerializeField] private float CircleReachTolerance = 0.7f;
    //[SerializeField] private float InvokeRepeatFrequency = 0.1f;
    [SerializeField] private float AroundCircleReachTolerance = 0.5f;
    [SerializeField] private float CrossingReachTolerance = 0.1f;
    [SerializeField] private float CollisionRadius = 1f;
    #endregion

    #region Private
    private int _currentWaypoint;
    private bool _reachedCircle = false;
    private bool _circleClockwise = true;
    private float _possitionAtCircle = 0;
    private Vector2 _defaultZeroPoint = Vector2.zero;
    private Vector2 _nextTarget = Vector2.zero;
    private bool _isCrossing = false;
    private Action _onCrossFinishAction;
    #endregion
    // Start is called before the first frame update
    private void Start()
    {
        this.Paused = false;
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
        }
        else if (!_reachedCircle)
        {
            MoveToCircle();
            CheckReachedCircle();
        }
        else
        {
            MoveToPointOnCircle();
            CheckFlyingAroundCircle();
        }
    }

    private void MoveToCircle()
    {
        var moveVector = GetDirectionToTarget() * MovementSpeed * Time.fixedDeltaTime;
        this.transform.position += new Vector3(moveVector.x, moveVector.y, 0);
    }

    private void MoveToCrossTarget()
    {
        var moveVector = GetDirectionToNextPoint() * MovementSpeed * Time.fixedDeltaTime;
        this.transform.position += new Vector3(moveVector.x, moveVector.y, 0);
    }

    private void MoveToPointOnCircle()
    {
        var moveVector =  GetDirectionToNextPoint() * MovementSpeed * Time.fixedDeltaTime;
        this.transform.position += new Vector3(moveVector.x, moveVector.y, 0);
    }

    private void CheckReachedCircle()
    {
        var distance = Vector2.Distance((Vector2)this.transform.position, (Vector2)Target.position);
        if(distance <= CircleRadius + CircleReachTolerance)
        {
            _defaultZeroPoint = GetDirectionFromTarget();
            _reachedCircle = true;
            GenerateNextTargetOnCircle();
        }
    }

    private void CheckReachedCrossingTarget()
    {
        var distance = Vector2.Distance((Vector2)this.transform.position, (Vector2)_nextTarget);
        if (distance <= CrossingReachTolerance)
        {
            _defaultZeroPoint = GetDirectionFromTarget();
            _possitionAtCircle = 0;
            _reachedCircle = true;
            _isCrossing = false;
            _onCrossFinishAction();
            GenerateNextTargetOnCircle();
        }
    }

    private void CheckFlyingAroundCircle()
    {
        var distance = Vector2.Distance((Vector2)this.transform.position, _nextTarget);
        if(Mathf.Abs(distance) <= AroundCircleReachTolerance)
        {
            GenerateNextTargetOnCircle();
        }
    }

    private void GenerateNextTargetOnCircle()
    {
        int maxItteration = Convert.ToInt32(360.0f / CircleStepSize);
        int i = 0;

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
                                        .Where(a=> a.gameObject.layer == (int)LayersNaming.LargeCollisionObjects)
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

    public bool Cross(Action onCrossFinish)
    {
        if (!_reachedCircle || _isCrossing)
            return false;

        _onCrossFinishAction = onCrossFinish;
        _isCrossing = true;
        _nextTarget = (Vector2)Target.position + GetDirectionToTarget() * CircleRadius;
        return true;
    }
}
