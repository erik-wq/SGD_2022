using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private GameObject LaserObject;
        [SerializeField] private Transform LaserSpriteTransform;
        [SerializeField] private Camera MainCamera;
        [SerializeField] private Animator PlayerAnimator;
        [SerializeField] private PlayerController PlayerControlScript;

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
        [SerializeField] private TMP_Text CooldownText;
        [SerializeField] private float ProjectileSpeed = 20;

        [SerializeField] private GameObject AimObject;
        //[SerializeField] private bool Knockback = false;
        //[SerializeField] private float KnockbackForce = 100;

        [SerializeField] private AudioSource AudioSourceComponent;
        [SerializeField] private AudioClip SpellAudio;

        [SerializeField] private Image CDImage;
        [SerializeField] private Image OOMImage;
        #endregion

        #region Private
        private float _startTime = 0;
        private bool _isActive = false;
        private event Action _onTurnedOff;
        private float _lastUsed = 0;
        private float _angle = 0;
        private float _angleOffset = 90;
        private bool _movingToPlayer = false;
        private Vector2 _laserOffset = new Vector2(0, 1.5f);
        #endregion

        void Start()
        {
        }

        private void Update()
        {

        }

        public void AdjustMana(float mana)
        {
            if (this.EnergyCost > mana)
            {
                OOMImage.enabled = true;
            }
            else
            {
                OOMImage.enabled = false;
            }
        }

        private void AdjustCooldown()
        {
            var perc = (Time.time - _lastUsed) / Cooldown;
            if (perc > 1)
                perc = 1;
            CDImage.fillAmount = 1 - perc;
        }

        private void FixedUpdate()
        {
            if (_movingToPlayer)
            {
                DrawHopeToPlayer();
            }
            else if (_isActive)
            {
                if (Time.time > _startTime + Length)
                {
                    HideGraphics();
                    TurnOffLaster();
                    ShowHope();
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

            AdjustCooldown();
        }

        private void AdjustCooldownText()
        {
            if (Time.time < _lastUsed + Cooldown)
            {
                var time = Math.Round(Time.time - (_lastUsed + Cooldown));
                //CooldownText.text = Convert.ToString(time);
            }
            else
            {
                //CooldownText.text = "";
            }
        }

        private void DrawHopeToPlayer()
        {
            if (HopesTransform.position != PlayersTransform.position)
            {
                var direction = ((Vector2)PlayersTransform.position - (Vector2)HopesTransform.position).normalized;
                var distance = Vector2.Distance((Vector2)PlayersTransform.position, (Vector2)HopesTransform.position);

                var move = direction * ProjectileSpeed * Time.deltaTime;

                if (move.magnitude < distance)
                {
                    var nextPossition = (Vector2)HopesTransform.position + move;
                    HopesTransform.position = new Vector3(nextPossition.x, nextPossition.y, HopesTransform.position.z);
                }
                else
                {
                    HopesTransform.position = PlayersTransform.position;
                    _movingToPlayer = false;
                    ActivateLaser();
                }
            }
            else
            {
                ActivateLaser();
            }
        }

        private void Rotate()
        {
            var direction = MathUtility.FullAngle(Vector2.up, CustomUtilities.GetMouseDirection(MainCamera, PlayersTransform));

            if (_angle != direction)
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
            PlayerControlScript.ActionLocked = false;
            PlayerController.IsMovementLocked = false;
            HopeScript.IsAbilityLocked = false;
            PlayerAnimator.SetBool("IsLaserOn", false);
        }

        private void HideGraphics()
        {
            LaserObject.SetActive(false);
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
                    IEnemy iEnemy = item.gameObject.GetComponent<IEnemy>();
                    if (iEnemy == null)
                    {
                        iEnemy = item.gameObject.GetComponentInParent<IEnemy>();
                    }

                    if (iEnemy == null)
                    {
                        iEnemy = item.transform.parent.GetComponentInChildren<IEnemy>();
                    }
                    iEnemy.TakeDamage(DamagePerSecond * Time.deltaTime);
                }
            }
        }

        private void ShowEffect()
        {
            var direction = MathUtility.RotateVector(Vector2.up, _angle);
            _laserOffset.x = (direction.x > 0) ? _laserOffset.x : -_laserOffset.x;
            LaserSpriteTransform.position = (Vector2)PlayersTransform.position + _laserOffset;
            LaserSpriteTransform.rotation = Quaternion.Euler(new Vector3(0, 0, _angle + _angleOffset));

            LaserObject.SetActive(true);
        }

        public bool Activate()
        {
            if (Time.time > _lastUsed + Cooldown)
            {
                HopeScript.IsAbilityLocked = true;
                TurnToProjectile();
                _movingToPlayer = true;
                return true;
            }
            return false;
        }

        private void TurnToProjectile()
        {
            HopesSprite.enabled = false;
            AimObject.SetActive(true);
        }

        private void TurnOffProjectile()
        {
            AimObject.SetActive(false);
        }

        private void ShowHope()
        {
            HopesSprite.enabled = true;
        }

        private void ActivateLaser()
        {
            TurnOffProjectile();
            _movingToPlayer = false;
            PlayerAnimator.SetBool("IsLaserOn", true);
            PlayerAnimator.Play("PlayerLaserPrepare");
            AudioSourceComponent.PlayOneShot(SpellAudio);
            var mouseDirection = CustomUtilities.GetMouseDirection(MainCamera, PlayersTransform);
            PlayerControlScript.AdjustFlip(mouseDirection);
            PlayerControlScript.ActionLocked = true;
            _lastUsed = Time.time;
            _startTime = Time.time;
            HopeScript.IsAbilityLocked = true;
            _isActive = true;
            _angle = MathUtility.FullAngle(Vector2.up, mouseDirection);
            ShowEffect();

            if (MovementLocked)
                PlayerController.IsMovementLocked = true;
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
