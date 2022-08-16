using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.TestingAssets.TestScripts.Follows
{
    class EnemyFollowFromRange : MonoBehaviour
    {
        #region Public/Serialized
        [SerializeField] public float ChangeFrequency = 4;
        [SerializeField] public float MaxDistance = 10;
        [SerializeField] public float MaxChange = 60;
        [SerializeField] public float ChangeStep = 10;

        [SerializeField] public float DistanceChangeCheck = 1.5f;
        [SerializeField] public float NumberOfDistanceChecks = 4;
        [SerializeField] private ContactFilter2D CollisionsFilter;
        #endregion

        #region Private
        private BasicFollow _mainFollow;
        private Transform _targetTransform;
        private Rigidbody2D _targetRigidBody;
        private Transform _pathTransform;
        private Transform _myTransform;

        private float _lastChange;
        private List<float> _anglesRandomized;
        #endregion

        public EnemyFollowFromRange(BasicFollow follow, Transform myTransform)
        {
            this._mainFollow = follow;
            this._myTransform = myTransform;
            CreatedAngles();
        }

        private void CreatedAngles()
        {
            float angle = 0;
            while (angle <= MaxChange / 2)
            {
                _anglesRandomized.Add(angle);
                angle += ChangeStep;
            }

            angle = 0;

            while (angle >= -MaxChange / 2)
            {
                _anglesRandomized.Add(angle);
                angle -= ChangeStep;
            }
        }

        private void RandomizeAngles()
        {
            _anglesRandomized = _anglesRandomized.OrderBy(a => UnityEngine.Random.Range(0, 10000)).ToList();
        }

        public void ChangeTarget(Transform target)
        {
            this._targetTransform = target;
        }

        private void PickNewPossition()
        {
            RandomizeAngles();
            foreach (var item in _anglesRandomized)
            {
                if(CheckAngle(item))
                {
                    return;
                }
            }
        }

        private bool CheckAngle(float angle)
        {
            for (int i = 0; i < NumberOfDistanceChecks; i++)
            {
                var myAngle = MathUtility.FullAngle(Vector2.up, GetDirectionFromTarget());
                var direction = MathUtility.RotateVector(Vector2.up, myAngle + angle);

                List<RaycastHit2D> outCollisions = new List<RaycastHit2D>();
                _targetRigidBody.Cast(direction,
                            CollisionsFilter,
                            outCollisions,
                            MaxDistance - DistanceChangeCheck * i
                          ); ;

                if(outCollisions.Count == 0)
                {
                    SetTarget(direction, MaxDistance - DistanceChangeCheck * i);
                    return true;
                }
            }

            return false;
        }

        private void SetTarget(Vector2 direction, float distance)
        {
            var possition = (Vector2)_targetTransform.position + (direction * distance);
            _pathTransform.position = new Vector3(possition.x, possition.y, 1);
            _mainFollow.SetTarget(_pathTransform);

        }

        private void FixedUpdate()
        {
            if (_targetTransform ==  null || Time.time > _lastChange + ChangeFrequency)
            {
                PickNewPossition();
            }
        }

        private Vector2 GetDirectionFromTarget()
        {
            return (_targetTransform.position - _myTransform.position).normalized;
        }
    }
}
