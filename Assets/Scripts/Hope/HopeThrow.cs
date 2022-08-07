using Assets.Scripts.Utils;
using Assets.TestingAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        [SerializeField] private SpriteRenderer HopeAimSprite;
        [SerializeField] private Camera MainCamera;
        [SerializeField] private SpriteRenderer IndicatorSprite;
        [SerializeField] private Transform IndicatorTransform;
        [SerializeField] private PlayerController PlayerControllerScript;
        [SerializeField] private HopeAI HopeScript;

        //Settings
        [SerializeField] private float EnergyCost;
        [SerializeField] private float MaxGrabRange = 15;
        [SerializeField] private float MaxShootRange = 25;
        [SerializeField] private float ProjectileSpeed = 50;
        #endregion

        #region Private
        private bool _isAiming = false;
        private bool _isFireing = false;
        private Vector2 _target;
        #endregion

        public void Start()
        {
            IndicatorSprite.enabled = false;
        }

        public void FixedUpdate()
        {
            if(_isAiming)
            {
                UpdateIndicator();
                DrawHopeToPlayer();
            }

            if(_isFireing)
            {
                if(MoveHopeToTarget())
                {
                    UnlockHope();
                }
            }
        }

        private void UnlockHope()
        {
            HopeScript.IsMovementLocked = false;
            HopeScript.IsHopeLocked = false;
            HopesSprite.enabled = true;
            HopeAimSprite.enabled = false;
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
            if(_isAiming)
            {
                if(HopesTransform.position != PlayersTransform.position)
                {
                    var direction = ((Vector2)PlayersTransform.position - (Vector2)HopesTransform.position).normalized;
                    var distance = Vector2.Distance((Vector2)PlayersTransform.position, (Vector2)HopesTransform.position);

                    var move = direction * ProjectileSpeed * Time.deltaTime;

                    if(move.magnitude < distance)
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
            _isAiming = true;
            IndicatorSprite.enabled = true;
            HopesSprite.enabled = false;
            HopeAimSprite.enabled = true;
            PlayerControllerScript.ActionLocked = true;
            HopeScript.IsMovementLocked = true;
        }

        public void MouseClick()
        {
            if (_isAiming)
                Fire();
        }

        public bool Activate()
        {
            if(!_isAiming)
            {
                if (CheckHopesDistance())
                {
                    StartAiming();
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
