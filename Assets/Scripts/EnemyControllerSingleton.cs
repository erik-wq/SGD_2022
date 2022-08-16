using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class EnemyControllerSingleton
    {
        private static EnemyControllerSingleton _instance = new EnemyControllerSingleton();

        #region Settings
        private const int MAX_FIRE_AT_ONCE = 1;
        private const float FIRE_DELAY = 2.5f;
        #endregion

        #region Private
        private List<EnemyAI> _activeEnemies = new List<EnemyAI>();
        private List<float> _lastFires = new List<float>();
        #endregion
        private EnemyControllerSingleton()
        {

        }

        public void Init()
        {
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
    }
}
