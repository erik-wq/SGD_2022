using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.TestingAssets.TestScripts.Hope
{
    class HopeLaser : MonoBehaviour, IHopeAbility
    {
        #region Serialized Fields
        //System variables
        [SerializeField] private Transform HopesTransform;
        [SerializeField] private Transform PlayersTransform;
        [SerializeField] private PlayerController PlayerController;
        [SerializeField] private SpriteRenderer HopesSprite;
        [SerializeField] private HopeAI HopeScript;
        [SerializeField] private SpriteRenderer LaserSprite;
        [SerializeField] private Transform LaserSpriteTransform;
        [SerializeField] private Camera MainCamera;

        //Design variables
        [SerializeField] private float Length = 3;
        [SerializeField] private float EnergyCost = 80;
        [SerializeField] private float Range = 50;
        [SerializeField] private float Width = 4;
        [SerializeField] private bool DirectionLocked = true;
        [SerializeField] private bool MovementLocked = true;
        [SerializeField] private float RotationSpeed = 15;
        [SerializeField] private float DamagePerSecond = 5;
        [SerializeField] private float Cooldown = 10;
        //[SerializeField] private bool Knockback = false;
        //[SerializeField] private float KnockbackForce = 100;
        #endregion

        #region Private
        private float _startTime = 0;
        private bool _isActive = false;
        private event Action _onTurnedOff;
        private float _angle = 0;
        #endregion

        void Start()
        {
        }
        
        private void Update()
        {

        }

        private void FixedUpdate()
        {
            if (_isActive)
            {
                if (Time.time > _startTime + Length)
                {
                    HideGraphics();
                    TurnOffLaster();
                    if (_onTurnedOff != null)
                    {
                        _onTurnedOff();
                    }
                }
                else
                {
                    if (!DirectionLocked)
                    {
                        Rotate();
                    }

                    DoDamage();
                }
            }
        }

        private void Rotate()
        {
            var direction = MathUtility.FullAngle(Vector2.up, CustomUtilities.GetMouseDirection(MainCamera, PlayersTransform));
            
            if(_angle != direction)
            {
                var rotation = (RotationSpeed * Time.deltaTime);
                _angle = MathUtility.NormalizeAngle(_angle);
                _angle = MathUtility.BringAnglesCloser(_angle, direction, rotation);
                if (Math.Abs(MathUtility.AngleDifference(_angle, direction)) < 1f)
                {
                    _angle = direction;
                }

                ShowEffect();
            }
        }

        private void TurnOffLaster()
        {
            _isActive = false;
            PlayerController.IsMovementLocked = false;
            HopeScript.IsAbilityLocked = false;
        }

        private void HideGraphics()
        {
            LaserSprite.enabled = false;
        }

        private void DoDamage()
        {
            var point = MathUtility.RotateVector(new Vector2(0, Range / 2.0f), _angle);
            point = point + (Vector2)PlayersTransform.position;
            var hits = Physics2D.OverlapBoxAll(point, new Vector2(Width, Range), _angle);

            foreach (var item in hits)
            {
                if (item.gameObject.tag == "EnemyHitCollider")
                {
                    IEnemy iEnemy = item.GetComponent<IEnemy>();
                    if (iEnemy == null)
                        iEnemy = item.GetComponentInParent<IEnemy>();
                    iEnemy.TakeDamage(DamagePerSecond * Time.deltaTime);
                }
            }
        }

        private void ShowEffect()
        {
            LaserSpriteTransform.rotation = Quaternion.Euler(new Vector3(0, 0, _angle));
            LaserSpriteTransform.position = PlayersTransform.position;
            LaserSprite.enabled = true;
        }

        public bool Activate()
        {
            _startTime = Time.time;
            HopeScript.IsAbilityLocked = true;
            _isActive = true;
            _angle = MathUtility.FullAngle(Vector2.up, CustomUtilities.GetMouseDirection(MainCamera, PlayersTransform));
            ShowEffect();

            if (MovementLocked)
                PlayerController.IsMovementLocked = true;

            return true;
        }

        public float GetCost()
        {
            return this.EnergyCost;
        }

        public bool IsLocked()
        {
            return _isActive;
        }

        public void BindOnTurnedOff(Action action)
        {
            _onTurnedOff += action;
        }
    }
}
