using Assets.Scripts.Utils;
using Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.TestingAssets.TestScripts
{
    public class ShadowSeekerFollow : MonoBehaviour, IShadowFollow
    {
        #region Public
        public bool Paused { get; set; }
        #endregion

        #region Serialized
        [SerializeField] private Transform target;
        [SerializeField] private string CollisionGraphMaskName = "Default";

        [SerializeField] private float MovementSpeed = 3f;
        [SerializeField] private float CollisionOffset = 1f;
        [SerializeField] private float NextWaypointDistance = 0.6f;
        [SerializeField] private float MaximumRotationSpeed = 180f;
        [SerializeField] private float RotationSpeedup = 15f;
        [SerializeField] private float RotationSpeed = 5f;
        [SerializeField] private float Acceleration = 3f;

        [SerializeField] private DebugControl DebugControl;
        [SerializeField] private Seeker Seeker;
        [SerializeField] private Transform LeadingPointDebug;
        #endregion

        #region Private
        private Func<float> _getRotation;
        private Func<float, float> _setRotation;

        private Path _path;
        private int _currentWaypoint = 0;
        private int _targetWaypoint = -1;
        private Rigidbody2D _rigidBody;
        private ContactFilter2D _contactFilter;
        private Collider2D _aggroCollider;
        private float _currentSpeed;
        private float _minimalSpeed = 0.01f;
        private float _currentRotationSpeed;
        #endregion

        public ShadowSeekerFollow(Func<float> getRotation, Func<float, float> setRotation, Seeker seeker, ContactFilter2D contactFilter, Collider2D aggroCollider)
        {
            this._currentRotationSpeed = RotationSpeed;
            this._getRotation = getRotation;
            this._setRotation = setRotation;
            this.Seeker = seeker;
            this._rigidBody = GetComponent<Rigidbody2D>();
            this._contactFilter = contactFilter;
            this._aggroCollider = aggroCollider;
        }

        public void Init(Func<float> getRotation, Func<float, float> setRotation, ContactFilter2D contactFilter, Collider2D aggroCollider, Rigidbody2D rigidBody)
        {
            this._getRotation = getRotation;
            this._setRotation = setRotation;
            this._rigidBody = rigidBody;
            this._contactFilter = contactFilter;
            this._aggroCollider = aggroCollider;
        }

        IShadowFollow IShadowFollow.CreateShadowFollow(Func<float> getRotation, Func<float, float> setRotation, Seeker seeker, ContactFilter2D contactFilter, Collider2D aggroCollider)
        {
            return new ShadowSeekerFollow(getRotation, setRotation, seeker, contactFilter, aggroCollider);
        }

        void Start()
        {
            InvokeRepeating("UpdatePath", 0f, 0.2f);
            _currentSpeed = MovementSpeed;
        }

        void UpdatePath()
        {
            if (target != null)
            {
                Seeker.StartPath(transform.position, target.position, OnPathComplete, GraphMask.FromGraphName(CollisionGraphMaskName));
            }
        }

        private void OnPathComplete(Path p)
        {
            if (!p.error)
            {
                _path = p;
                _targetWaypoint = -1;
                _currentWaypoint = 0;
            }
        }

        private void FixedUpdate()
        {
            if (Paused || _path == null)
                return;

            FindNextWaypoint();
            if (LeadingPointDebug != null)
            {
                LeadingPointDebug.position = _path.vectorPath[_targetWaypoint];
            }

            var direction = CalculateMovingDirection();
            AdjustSpeed(direction);
            var collisions = TryMove(direction);
            if (collisions.Count == 0)
            {
                Move(direction);
                _currentRotationSpeed = RotationSpeed;
            }
            else
            {
                SpeedUpRotation();
            }

            float distance = Vector2.Distance(_rigidBody.position, _path.vectorPath[_targetWaypoint]);
            if (distance < NextWaypointDistance)
            {
                _currentWaypoint = _targetWaypoint;
            }
        }

        private void Move(Vector2 direction)
        {
            _rigidBody.MovePosition(_rigidBody.position + direction * GetMoveDistance());
        }

        private void SpeedUpRotation()
        {
            if (_currentRotationSpeed < MaximumRotationSpeed)
            {
                _currentRotationSpeed += Time.deltaTime * RotationSpeedup;
            }
        }

        private void AdjustSpeed(Vector2 direction)
        {
            var targetDirection = ((Vector2)(_path.vectorPath[_targetWaypoint] - transform.position)).normalized;
            var angleDifference = Vector2.Angle(direction, targetDirection);

            var timeToRotate = angleDifference / _currentRotationSpeed;
            var distance = Vector2.Distance(this.transform.position, _path.vectorPath[_targetWaypoint]);
            var distanceTravelTime = distance / _currentSpeed;

            if (timeToRotate > distanceTravelTime)
            {
                var targetSpeed = distance / timeToRotate;

                var speedDifference = _currentSpeed - targetSpeed;
                var decSpeed = speedDifference / distanceTravelTime;
                _currentSpeed -= decSpeed * Time.fixedDeltaTime;

                if (_currentSpeed < _minimalSpeed)
                    _currentSpeed = _minimalSpeed;
            }
            else
            {
                if (_currentSpeed < MovementSpeed)
                {
                    _currentSpeed += Acceleration * Time.fixedDeltaTime;
                }

                if (_currentSpeed > MovementSpeed)
                {
                    _currentSpeed = MovementSpeed;
                }
            }

            //DebugControl.SetShadeSpeed(_currentSpeed);
        }

        private bool FindNextWaypoint()
        {
            if (_currentWaypoint == _path.vectorPath.Count)
                return false;

            _aggroCollider.enabled = false;
            Physics2D.queriesHitTriggers = false;
            for (int i = _currentWaypoint; i < _path.vectorPath.Count; i++)
            {
                var direction = (_path.vectorPath[i] - this.transform.position).normalized;
                var distance = Vector2.Distance(_path.vectorPath[i], this.transform.position);

                var collisions = new List<RaycastHit2D>();

                int collisionsCount = _rigidBody.Cast(direction,
                                        _contactFilter,
                                        collisions,
                                        distance
                                      );

                if (collisionsCount != 0)
                {
                    _aggroCollider.enabled = true;
                    Physics2D.queriesHitTriggers = true;
                    if (i != 0)
                        i--;
                    this._targetWaypoint = i;
                    return true;
                }
            }

            _targetWaypoint = _path.vectorPath.Count - 1;
            _aggroCollider.enabled = true;
            Physics2D.queriesHitTriggers = true;

            return false;
        }

        private List<RaycastHit2D> TryMove(Vector2 direction)
        {
            List<RaycastHit2D> outCollisions = new List<RaycastHit2D>();

            Physics2D.queriesHitTriggers = false;
            _aggroCollider.enabled = false;
            _rigidBody.Cast(direction,
                        _contactFilter,
                        outCollisions,
                        GetMoveDistance() + CollisionOffset
                      );

            Physics2D.queriesHitTriggers = true;
            _aggroCollider.enabled = true;
            return SoftUnstuck(direction, outCollisions);
        }

        private List<RaycastHit2D> SoftUnstuck(Vector2 direction, List<RaycastHit2D> collisions)
        {
            List<RaycastHit2D> relevantCollisions = new List<RaycastHit2D>();
            foreach (var item in collisions)
            {
                var toObject = ((Vector2)item.transform.position - (Vector2)_rigidBody.transform.position).normalized;
                var dot = Vector2.Dot(toObject, direction);
                if (Vector2.Dot(toObject, direction) > 0)
                {
                    relevantCollisions.Add(item);
                }
            }

            return relevantCollisions;
        }

        private Vector2 CalculateMovingDirection()
        {
            Vector2 desiredDirection = ((Vector2)(_path.vectorPath[_targetWaypoint] - this.transform.position)).normalized;
            Vector2 facingDirection = GetFacingDirection();

            if (desiredDirection != facingDirection)
            {
                var currenctAngle = Vector2.SignedAngle(Vector2.up, GetFacingDirection());
                var desiredDirectionAngle = Vector2.SignedAngle(Vector2.up, desiredDirection);

                var rotation = _currentRotationSpeed * Time.fixedDeltaTime;

                float targetAngle = 0;
                if (Math.Abs(currenctAngle - desiredDirectionAngle) > 180)
                {
                    targetAngle = currenctAngle - ((desiredDirectionAngle > currenctAngle) ? rotation : -rotation);
                }
                else
                {
                    targetAngle = currenctAngle + ((desiredDirectionAngle > currenctAngle) ? rotation : -rotation);
                }

                facingDirection = MathUtility.RotateVector(Vector2.up, targetAngle).normalized;
                if (currenctAngle < desiredDirectionAngle && _currentSpeed + rotation > desiredDirectionAngle)
                {
                    facingDirection = desiredDirection;
                }

                _setRotation(targetAngle);
                return facingDirection;
            }

            return Vector2.up;
        }

        private Vector2 GetFacingDirection()
        {
            return MathUtility.RotateVector(Vector2.up, _getRotation()).normalized;
        }

        private float GetMoveDistance()
        {
            return _currentSpeed * Time.fixedDeltaTime;
        }

        public void SetTarget(Transform transform)
        {
            this.target = transform;
        }
    }
}
