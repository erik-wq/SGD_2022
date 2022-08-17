﻿using System;
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
    }
}
