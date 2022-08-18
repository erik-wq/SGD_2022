using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Totems
{
    class TotemSpawner : MonoBehaviour
    {
        #region Public
        public delegate void OnSpawnedDelegate(GameObject obj);
        public float SpawnTime = 2.5f;
        public GameObject SpawnObject;
        [SerializeField] private GameObject LightingObject;
        #endregion

        #region Private
        private SpriteRenderer _fogSprite;
        private float _startTime = 0;
        private bool _isLightingSpawned = false;
        private event OnSpawnedDelegate _onSpawned;
        #endregion

        private void Start()
        {
            _fogSprite = GetComponent<SpriteRenderer>();
            _fogSprite.color = new Color(1, 1, 1, 0);
            _startTime = Time.time;
        }

        public void RegisterOnSpawned(OnSpawnedDelegate action)
        {
            _onSpawned += action;
        }

        private void FixedUpdate()
        {
            if(Time.time < _startTime + SpawnTime)
            {
                AdjustVisibility();
            }
            else if(!_isLightingSpawned)
            {
                SpawnLighting();
            }
        }

        private void SpawnLighting()
        {
            LightingObject.SetActive(true);
            var script = LightingObject.GetComponent<LightningArea>();
            script.SpawnThunder(this.transform.position);
            script.RegisterOnDamageFinished(OnLightingFinishes);
        }

        private void OnLightingFinishes()
        {
            if (SpawnObject != null)
            {
                var obj = Instantiate(SpawnObject, this.transform.position, Quaternion.identity);
                _onSpawned(obj);
            }

            Wait();
            Destroy(this.gameObject);
        }

        IEnumerator Wait()
        {
            yield return new WaitForSecondsRealtime(1.5f);
        }

        private void AdjustVisibility()
        {
            var perc = Time.time / (_startTime + SpawnTime);
            if (perc > 1)
                perc = 1;

            _fogSprite.color =  new Color(1, 1, 1, perc);
        }
    }
}
