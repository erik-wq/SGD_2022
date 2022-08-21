using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class EnemyControllerSingleton : MonoBehaviour
    {
        private static EnemyControllerSingleton _instance = new EnemyControllerSingleton();

        public static EnemyControllerSingleton Instance
        {
            get { return _instance; }
        }

        #region Settings
        private const int MAX_FIRE_AT_ONCE = 1;
        private const float FIRE_DELAY = 3f;
        #endregion

        #region Private
        private List<EnemyAI> _activeEnemies = new List<EnemyAI>();
        private List<float> _lastFires = new List<float>();

        private float _lastMeleeDamaged;
        private float _lastFlyingDamaged;
        private float _lastMeleeDying;
        private float _lastFlyingDying;
        private float _lastMotherDamage;
        #endregion
        private EnemyControllerSingleton()
        {
            Init();
        }

        private void Awake()
        {
            // If there is an instance, and it's not me, delete myself.

            if (_instance != null && _instance != this)
            {
                Destroy(this);
            }
            else
            {
                _instance = this;
                _instance.Init();
            }
        }

        private void Init()
        {
            _lastMeleeDamaged = 0;
            _lastMeleeDying = 0;
            _lastFlyingDamaged = 0;
            _lastFlyingDying = 0;

            _lastFires = new List<float>();
            for (int i = 0; i < MAX_FIRE_AT_ONCE; i++)
            {
                _lastFires.Add(0);
            }
        }

        public static EnemyControllerSingleton GetInstance()
        {
            return _instance;
        }

        public void Register(EnemyAI enemy)
        {
            if (!_activeEnemies.Contains(enemy))
                _activeEnemies.Add(enemy);
        }

        public void Unregister(EnemyAI enemy)
        {
            _activeEnemies.Remove(enemy);
        }

        public bool AskToFire()
        {
            for (int i = 0; i < _lastFires.Count; i++)
            {
                if (Time.time > _lastFires[i] + FIRE_DELAY)
                {
                    _lastFires[i] = Time.time;
                    return true;
                }
            }

            return false;
        }

        public bool CanPlayMeleeDamage()
        {
            if(Time.time > _lastMeleeDamaged + 2.5f)
            {
                _lastMeleeDamaged = Time.time;
                return true;
            }
            return false;
        }

        public bool CanPlayMeeleDeath()
        {
            if (Time.time > _lastMeleeDying + 2.5f)
            {
                _lastMeleeDying = Time.time;
                return true;
            }
            return false;
        }

        public bool CanPlayFlyingDeath()
        {
            if (Time.time > _lastFlyingDying + 2.5f)
            {
                _lastFlyingDying = Time.time;
                return true;
            }
            return false;
        }

        public bool CanPlayFlyingDamage()
        {
            if (Time.time > _lastFlyingDamaged + 2.5f)
            {
                _lastFlyingDamaged = Time.time;
                return true;
            }
            return false;
        }

        public bool CanPlayMotherDamage()
        {
            if (Time.time > _lastMotherDamage + 2.5f)
            {
                _lastMotherDamage = Time.time;
                return true;
            }
            return false;
        }
    }
}
