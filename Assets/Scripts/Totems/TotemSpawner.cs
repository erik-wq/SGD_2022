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
        public float SpawnTime = 1f;
        public GameObject SpawnMeele;
        public AudioClip ThunderAudioClip;
        public AudioSource MainAudioSource;
        #endregion

        #region Private
        private SpriteRenderer _fogSprite;
        private float _startTime = 0;
        private bool _isLightingSpawned = false;
        private event OnSpawnedDelegate _onSpawned;
        private bool _initCompleted = false;
        private GameObject LightingObject;
        #endregion

        private void Start()
        {
            LightingObject = this.transform.Find("Thunder").gameObject;
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
            if (Time.time < _startTime + SpawnTime)
            {
                AdjustVisibility();
            }
            else if (!_isLightingSpawned)
            {
                AdjustVisibility();
                SpawnLighting();
            }
        }

        private void SpawnLighting()
        {
            _isLightingSpawned = true;
            var script = LightingObject.GetComponent<Thunder>();
            script.OnThunderEnds += OnLightingFinishes;
            MainAudioSource.PlayOneShot(ThunderAudioClip);
            LightingObject.SetActive(true);
        }

        private void OnLightingFinishes()
        {
            var obj = Instantiate(SpawnMeele, this.transform.position, Quaternion.identity);
            _onSpawned(obj);
            StartCoroutine(Wait());
        }

        IEnumerator Wait()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            Destroy(this.gameObject);
        }

        private void AdjustVisibility()
        {
            var perc = (Time.time - _startTime) / SpawnTime;
            if (perc > 1)
                perc = 1;

            _fogSprite.color = new Color(1, 1, 1, perc);
        }
    }
}
