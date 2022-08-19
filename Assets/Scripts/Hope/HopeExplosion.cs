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
    class HopeExplosion : MonoBehaviour, IHopeAbility
    {
        #region Serialized
        //System
        [SerializeField] private Animator HopesAnimator;
        [SerializeField] private Animator EffectsAnimator;
        [SerializeField] private float Cooldown;
        [SerializeField] private float AnimationLockLength = 1.5f;
        [SerializeField] private float ExplosionRadius = 10;
        [SerializeField] private float ExplosionDamage = 15;
        [SerializeField] private TMP_Text CooldownText;

        //GD
        [SerializeField] private float Cost;
        #endregion

        #region Private
        private float _lastUsedTime;
        private HopeAI _hopeIA;
        private bool _isExploding = false;
        #endregion

        public void Start()
        {
            _hopeIA = GetComponent<HopeAI>();
        }

        public void FixedUpdate()
        {
            if(_isExploding)
            {
                if(Time.time > _lastUsedTime + AnimationLockLength)
                {
                    _isExploding = false;
                }
            }

            AdjustCooldownText();
        }

        private void AdjustCooldownText()
        {
            if (Time.time < _lastUsedTime + Cooldown)
            {
                var time = Math.Round(Time.time - (_lastUsedTime + Cooldown));
                CooldownText.text = Convert.ToString(time);
            }
            else
            {
                CooldownText.text = "";
            }
        }

        public bool Activate()
        {
            if (Time.time > _lastUsedTime + Cooldown)
            {
                _lastUsedTime = Time.time;
                HopesAnimator.Play("HopeIsCharging");
                _isExploding = true;
                return true;
            }
            return false;
        }

        public void ExplodeFromAnimation()
        {
            EffectsAnimator.Play("Hope_Explosion");
            DamageEnemies();
        }

        private void DamageEnemies()
        {
            var hits = Physics2D.OverlapCircleAll(_hopeIA.transform.position, ExplosionRadius);

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

                    iEnemy.TakeDamage(ExplosionDamage);
                }
            }
        }

        public float GetCost()
        {
            return this.Cost;
        }
    }
}
