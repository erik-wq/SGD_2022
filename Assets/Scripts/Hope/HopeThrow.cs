using Assets.Scripts.Utils;
using Assets.TestingAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Hope
{
    class HopeThrow : MonoBehaviour, IHopeAbility
    {
        #region Serialized Fields
        //System
        [SerializeField] private Transform HopesTransform;
        [SerializeField] private Transform PlayersTransform;
        [SerializeField] private SpriteRenderer HopesSprite;
        [SerializeField] private GameObject AimObject;
        [SerializeField] private Camera MainCamera;
        [SerializeField] private SpriteRenderer IndicatorSprite;
        [SerializeField] private Transform IndicatorTransform;
        [SerializeField] private PlayerController PlayerControllerScript;
        [SerializeField] private HopeAI HopeScript;

        //Settings
        [SerializeField] private float EnergyCost = 25;
        [SerializeField] private float MaxGrabRange = 15;
        [SerializeField] private float MaxShootRange = 25;
        [SerializeField] private float ProjectileSpeed = 50;
        [SerializeField] private float Cooldown = 10;
        [SerializeField] private TMP_Text CooldownText;

        [SerializeField] private float PullLength = 4;
        [SerializeField] private float PullForce = 400;
        [SerializeField] private float PullForceTick = 0.8f;
        [SerializeField] private float PullRadius = 25;
        [SerializeField] private float PullRadiusForDamage = 15;
        [SerializeField] private float PullRadiusDamagePerTick = 10;

        [SerializeField] private AudioSource AudioSourceComponent;
        [SerializeField] private AudioClip SpellAudio;
        #endregion

        #region Private
        private bool _isAiming = false;
        private bool _isFireing = false;
        private Vector2 _target;
        private float _lastUsed = 0;

        private bool _isHaloActive = false;
        private bool _hasHaloFinished = false;
        private float _haloStarted = 0;
        private float _lastTick = 0;
        #endregion

        public void Start()
        {
            IndicatorSprite.enabled = false;
        }

        public void FixedUpdate()
        {
            if (_isAiming)
            {
                UpdateIndicator();
                DrawHopeToPlayer();
            }

            if (_isFireing)
            {
                if (MoveHopeToTarget())
                {
                    ActivateHalo();
                }
            }

            if (_isHaloActive)
            {
                HaloTick();
            }

            AdjustCooldownText();
        }

        private void ActivateHalo()
        {
            HopeScript.MakeInvulnerable();
            _isHaloActive = true;
            _hasHaloFinished = false;
            _isFireing = false;
            _haloStarted = Time.time;
        }

        private void HaloTick()
        {
            if (Time.time > _lastTick + PullForceTick)
            {
                Pull();
                PullDamage();
                _lastTick = Time.time;
            }

            if (Time.time > _haloStarted + PullLength)
            {
                _isHaloActive = false;
                _hasHaloFinished = true;
                UnlockHope();
                UnlockEnemies();
            }
        }

        private void UnlockEnemies()
        {
            var enemies = Physics2D.OverlapCircleAll(this.transform.position, PullRadius).ToList();
            enemies = enemies.Where(a => a.tag == "Enemy").ToList();

            foreach (var item in enemies)
            {
                var direction = (this.transform.position - item.transform.position).normalized;
                var rigidBody = item.GetComponent<Rigidbody2D>();
                if (rigidBody != null)
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

                    if (iEnemy is EnemyAI)
                    {
                        iEnemy.UnPauseFollow();
                        iEnemy.ClearForces();
                    }
                }
            }
        }

        private void Pull()
        {
            var enemies = Physics2D.OverlapCircleAll(this.transform.position, PullRadius).ToList();
            enemies = enemies.Where(a => a.tag == "Enemy").ToList();

            foreach (var item in enemies)
            {
                var direction = (this.transform.position - item.transform.position).normalized;
                var rigidBody = item.GetComponent<Rigidbody2D>();
                if (rigidBody != null)
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

                    if (iEnemy is EnemyAI)
                    {
                        iEnemy.PauseFollow();
                        item.GetComponent<Rigidbody2D>().AddForce(PullForce * direction, ForceMode2D.Force);
                    }
                }
            }
        }

        private void PullDamage()
        {
            var enemies = Physics2D.OverlapCircleAll(this.transform.position, PullRadiusForDamage).ToList();
            enemies = enemies.Where(a => a.tag == "Enemy").ToList();

            foreach (var item in enemies)
            {
                var rigidBody = item.GetComponent<Rigidbody2D>();
                if (rigidBody != null)
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

                    iEnemy.TakeDamage(PullRadiusDamagePerTick);
                }
            }
        }

        private void AdjustCooldownText()
        {
            if (Time.time < _lastUsed + Cooldown)
            {
                var time = Math.Round(Time.time - (_lastUsed + Cooldown));
                CooldownText.text = Convert.ToString(time);
            }
            else
            {
                CooldownText.text = "";
            }
        }

        private void UnlockHope()
        {
            HopeScript.IsMovementLocked = false;
            HopeScript.IsHopeLocked = false;
            HopesSprite.enabled = true;
            HopeScript.MakeVulnerable();
            AimObject.SetActive(false);
            _isFireing = false;
        }

        private bool MoveHopeToTarget()
        {
            if ((Vector2)HopesTransform.position != _target)
            {
                var direction = (_target - (Vector2)HopesTransform.position).normalized;
                var distance = Vector2.Distance(_target, (Vector2)HopesTransform.position);

                var move = direction * ProjectileSpeed * Time.deltaTime;

                if (move.magnitude < distance)
                {
                    var nextPossition = (Vector2)HopesTransform.position + move;
                    HopesTransform.position = new Vector3(nextPossition.x, nextPossition.y, HopesTransform.position.z);
                    return false;
                }
                else
                {
                    HopesTransform.position = _target;
                    return true;
                }
            }
            return true;
        }

        private void DrawHopeToPlayer()
        {
            if (_isAiming)
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
                    }
                }
            }
        }

        private void Fire()
        {
            _target = GetMouseTarget();
            AudioSourceComponent.PlayOneShot(SpellAudio);
            HopeScript.IsAbilityLocked = false;
            PlayerControllerScript.ActionLocked = false;
            _isFireing = true;
            _isAiming = false;
            IndicatorSprite.enabled = false;
        }

        private bool CheckHopesDistance()
        {
            return Vector2.Distance((Vector2)HopesTransform.position, (Vector2)PlayersTransform.position) <= MaxGrabRange;
        }

        private void UpdateIndicator()
        {
            IndicatorTransform.position = GetMouseTarget();
        }

        private Vector2 GetMouseTarget()
        {
            var cursorPossition = CustomUtilities.GetMousePossition(MainCamera);
            if (Vector2.Distance(cursorPossition, PlayersTransform.position) > MaxGrabRange)
            {
                var direction = (cursorPossition - (Vector2)PlayersTransform.position).normalized;
                var possition = (Vector2)PlayersTransform.position + (direction * MaxGrabRange);
                return possition;
            }
            else
            {
                return cursorPossition;
            }
        }

        private void StartAiming()
        {
            if (Time.time > _lastUsed + Cooldown)
            {
                _lastUsed = Time.time;
                _isAiming = true;
                IndicatorSprite.enabled = true;
                HopesSprite.enabled = false;
                AimObject.SetActive(true);
                PlayerControllerScript.ActionLocked = true;
                HopeScript.IsMovementLocked = true;
            }
        }

        public void MouseClick()
        {
            if (_isAiming)
                Fire();
        }

        public bool Activate()
        {
            if (!_isAiming && Time.time > _lastUsed + Cooldown)
            {
                if (CheckHopesDistance())
                {
                    StartAiming();
                    AudioSourceComponent.PlayOneShot(SpellAudio);
                    HopeScript.IsAbilityLocked = true;
                    HopeScript.IsHopeLocked = true;
                    return true;
                }
                else
                {
                    //Show not in distnace somehow
                }
            }

            return false;
        }

        public float GetCost()
        {
            return EnergyCost;
        }

        public bool IsLocked()
        {
            return false;
        }

        public void CancelAction()
        {
            //_isAiming = false;
            //IndicatorSprite.enabled = false;
        }
    }
}
