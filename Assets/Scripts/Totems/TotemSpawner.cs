using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Totems
{
    class TotemSpawner : MonoBehaviour
    {
        #region Serialized
        [SerializeField] private float SpawnTime = 2.5f;
        #endregion

        #region Private
        private SpriteRenderer _fogSprite;
        private float _startTime = 0;
        #endregion

        private void Start()
        {
            _fogSprite = GetComponent<SpriteRenderer>();
            _startTime = Time.time;
        }

        private void FixedUpdate()
        {
            if(Time.time < _startTime + SpawnTime)
            {
                AdjustVisibility();
            }
        }

        private void AdjustVisibility()
        {

        }
    }
}
