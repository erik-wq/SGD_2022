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
    class HopeEventHandler : MonoBehaviour
    {
        [SerializeField] private HopeExplosion _explosionScript;

        public void Awake()
        {
            
        }

        public void RunExplosion()
        {
            _explosionScript.ExplodeFromAnimation();
        }

    }
}
