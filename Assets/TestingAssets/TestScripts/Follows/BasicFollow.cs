using Assets.Scripts.Utils;
using Assets.TestingAssets.TestScripts;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicFollow : MonoBehaviour, IFollow
{
    #region Public
    [SerializeField] public Transform Target;
    public delegate void OnMoveAction(Vector2 direction);
    public delegate void OnStopMovingAction();
    public bool Paused { get; set; }
    public Path path
    {
        get { return _path; }
    }
    public float minDistance { get { return this.MinimalDistance; } }
    #endregion

    #region Serialized
    [SerializeField] private float MovementSpeed = 3f;
    [SerializeField] private float CollisionOffset = 1f;
    [SerializeField] private float NextWaypointDistance = 0.6f;
    [SerializeField] private float MinimalDistance = 1.3f;
    [SerializeField] private ContactFilter2D CollisionsFilter;
    [SerializeField] private Collider2D IgnoredCollider;
    [SerializeField] private bool ForceBased = false;
    [SerializeField] private float ForceModifier = 50;
    [SerializeField] private int AngleSplitForCollisionAvoiding = 12;
    [SerializeField] private string CollisionGraphMaskName = "Default";
    #endregion

    #region Private
    private Rigidbody2D _rigidBody2D;
    private Seeker _seeker;
    private Path _path;
    private int _currentWaypoint;
    private Animator _animator;
    private event OnMoveAction _onMove;
    private event OnStopMovingAction _onStopMoving;
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _seeker = GetComponent<Seeker>();
        _animator = GetComponent<Animator>();
        this.Paused = false;
        InvokeRepeating("UpdatePath", 0f, 0.5f);
    }

    void UpdatePath()
    {
        if (Target != null)
        {
            //_seeker.graphMask = (1 << 0) | (1 << 1);
            _seeker.StartPath(_rigidBody2D.position, Target.position, OnPathComplete, GraphMask.FromGraphName(CollisionGraphMaskName));
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        UpdatePathfinding();
    }

    private void UpdatePathfinding()
    {
        if (_path == null
            || Target == null
            || _currentWaypoint >= _path.vectorPath.Count
            || Paused)
        {
            return;
        }

        Vector2 direction = GetDirection();


        if (!CheckNextDistance(direction))
        {
            if(_onStopMoving != null)
            {
                _onStopMoving();
            }

            return;
        }

        if (IgnoredCollider != null)
            IgnoredCollider.enabled = false;

        List<RaycastHit2D> foundCollisions = TryMove(direction);

        if (foundCollisions.Count == 0)
        {
            Move(direction);
        }
        else
        {
            if (foundCollisions[0].transform.gameObject.tag == "CollisionObject")
            {
                Vector2 freeDirection = CheckAxisDirections(direction);
                if (freeDirection != Vector2.zero)
                    Move(freeDirection);
            }
            else if (foundCollisions[0].transform.gameObject.layer == (int)LayersNaming.Characters)
            {
                Vector2 freeDirection = TryAvoidingCharacter(direction);
                if (freeDirection != Vector2.zero)
                    Move(freeDirection);
            }
        }

        float distance = Vector2.Distance(_rigidBody2D.position, _path.vectorPath[_currentWaypoint]);
        if (distance < NextWaypointDistance)
        {
            _currentWaypoint++;
        }

        if (IgnoredCollider != null)
            IgnoredCollider.enabled = true;
    }

    private List<RaycastHit2D> TryMove(Vector2 direction)
    {
        List<RaycastHit2D> outCollisions = new List<RaycastHit2D>();

        Physics2D.queriesHitTriggers = false;
        _rigidBody2D.Cast(direction,
                    CollisionsFilter,
                    outCollisions,
                    GetMoveDistance() + CollisionOffset
                  );

        Physics2D.queriesHitTriggers = true;
        return SoftUnstuck(direction, outCollisions);
    }

    private List<RaycastHit2D> SoftUnstuck(Vector2 direction, List<RaycastHit2D> collisions)
    {
        List<RaycastHit2D> relevantCollisions = new List<RaycastHit2D>();
        foreach (var item in collisions)
        {
            var toObject = ((Vector2)item.transform.position - (Vector2)_rigidBody2D.transform.position).normalized;
            var dot = Vector2.Dot(toObject, direction);
            if (Vector2.Dot(toObject, direction) > 0)
            {
                relevantCollisions.Add(item);
            }
        }

        return relevantCollisions;
    }

    private Vector2 TryAvoidingCharacter(Vector2 direction)
    {
        float degreeStep = 180.0f / AngleSplitForCollisionAvoiding;
        int i = 1;
        while (i < AngleSplitForCollisionAvoiding)
        {
            float rotation = i * degreeStep;
            Vector2 rotatedDirection = MathUtility.RotateVector(direction, rotation);

            List<RaycastHit2D> outCollisions = TryMove(rotatedDirection);
            if (outCollisions.Count == 0)
                return rotatedDirection;

            rotatedDirection = MathUtility.RotateVector(direction, -rotation);

            outCollisions = TryMove(rotatedDirection);
            if (outCollisions.Count == 0)
                return rotatedDirection;

            i++;
        }

        //return (Vector2)this.transform.position;
        return Vector2.zero;
    }

    private Vector2 CheckAxisDirections(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x != 0)
            {
                var returnVector = TryMoveX(direction);
                if (returnVector != Vector2.zero)
                    return returnVector;
            }

            if (direction.y != 0)
            {
                var returnVector = TryMoveY(direction);
                if (returnVector != Vector2.zero)
                    return returnVector;
            }
        }
        else
        {
            if (direction.y != 0)
            {
                var returnVector = TryMoveY(direction);
                if (returnVector != Vector2.zero)
                    return returnVector;
            }

            if (direction.x != 0)
            {
                var returnVector = TryMoveX(direction);
                if (returnVector != Vector2.zero)
                    return returnVector;
            }
        }

        return Vector2.zero;
    }

    private Vector2 TryMoveX(Vector2 direction)
    {
        if (direction.x > 0)
        {
            var collisions = TryMove(new Vector2(1, 0));
            if (collisions.Count == 0)
                return new Vector2(1, 0);
        }
        else
        {
            var collisions = TryMove(new Vector2(-1, 0));
            if (collisions.Count == 0)
                return new Vector2(-1, 0);
        }
        return Vector2.zero;
    }

    private Vector2 TryMoveY(Vector2 direction)
    {
        if (direction.y > 0)
        {
            var collisions = TryMove(new Vector2(0, 1));
            if (collisions.Count == 0)
                return new Vector2(0, 1);
        }
        else
        {
            var collisions = TryMove(new Vector2(0, -1));
            if (collisions.Count == 0)
                return new Vector2(0, -1);
        }
        return Vector2.zero;
    }

    private Vector2 GetDirection()
    {
        return ((Vector2)_path.vectorPath[_currentWaypoint] - _rigidBody2D.position).normalized;
    }

    private float GetMoveDistance()
    {
        return MovementSpeed * Time.fixedDeltaTime;
    }

    private bool CheckNextDistance(Vector2 direction)
    {
        var nextPossition = _rigidBody2D.position + direction * MovementSpeed * Time.fixedDeltaTime;
        var nextDistance = Vector2.Distance(nextPossition, Target.position);
        return nextDistance >= MinimalDistance;
    }

    private void Move(Vector2 direction)
    {
        if (!ForceBased)
        {
            _rigidBody2D.MovePosition(_rigidBody2D.position + direction * GetMoveDistance());
        }
        else
        {
            _rigidBody2D.AddForce(direction * GetMoveDistance() * ForceModifier);
        }

        if (_onMove != null)
        {
            _onMove(direction);
        }
    }

    public void BindOnMove(OnMoveAction action)
    {
        _onMove += action;
    }

    public void BindOnStopMoving(OnStopMovingAction action)
    {
        _onStopMoving += action;
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            _path = p;
            _currentWaypoint = 0;
        }
    }

    public void SetTarget(Transform transform)
    {
        this.Target = transform;
    }
}
