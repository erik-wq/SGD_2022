using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.TestingAssets.TestScripts
{
    class HeavyEnemyAI : EnemyAI
    {
        #region Serialized
        [SerializeField] protected int ChargeCooldown = 10;
        [SerializeField] protected float ChargePower = 500;
        [SerializeField] protected float ChargeDuration = 2;
        [SerializeField] protected ContactFilter2D CollisionsFilter;
        [SerializeField] private Collider2D IgnoredCollider;
        #endregion

        #region Private
        protected float _chargeStartTime;
        protected float _lastCharge;
        protected bool _isCharging = false;

        #endregion

        protected new void Start()
        {
            _followScript = GetComponent<BasicFollow>();
            base.Start();
        }

        protected new void Update()
        {

        }

        protected new void FixedUpdate()
        {
            if (CheckLos() && !_isCharging && Time.time > _lastCharge + ChargeCooldown)
            {
                Charge();
            }
            else if (!_isCharging)
            {
                base.FixedUpdate();
            }
            else if (Time.time - _chargeStartTime > ChargeDuration)
            {
                StopCharge();
            }
        }

        private void StopCharge()
        {
            _rigidBody.velocity = Vector2.zero;
            _rigidBody.angularVelocity = 0;

            _isCharging = false;
            _followScript.Paused = false;
            _lastCharge = Time.time;
        }

        private void Charge()
        {
            _isCharging = true;
            _followScript.Paused = true;
            _chargeStartTime = Time.time;

            Vector2 playerPossition = new Vector2(PlayerTransform.position.x, PlayerTransform.position.y);
            Vector2 direction = (playerPossition - _rigidBody.position).normalized;
            Vector2 force = direction * ChargePower;
            _rigidBody.AddForce(force, ForceMode2D.Impulse);
        }

        private bool CheckLos()
        {
            IgnoredCollider.enabled = false;
            List<RaycastHit2D> outCollisions = new List<RaycastHit2D>();
            Vector2 playerPossition = new Vector2(PlayerTransform.position.x, PlayerTransform.position.y);
            Vector2 direction = (playerPossition - _rigidBody.position).normalized;
            float distance = Vector2.Distance(playerPossition, _rigidBody.position);

            _rigidBody.Cast(direction,
                            CollisionsFilter,
                            outCollisions,
                            distance);

            outCollisions = outCollisions.Where(a => a.transform.tag != "Player").ToList();
            IgnoredCollider.enabled = true;
            return outCollisions.Count == 0;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if(collision.transform.gameObject.tag == "Player")
            {
                StopCharge();
            }
        }
    }
}
