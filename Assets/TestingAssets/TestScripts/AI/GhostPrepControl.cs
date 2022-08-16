using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.TestingAssets.TestScripts.AI
{
    class GhostPrepControl : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        private Transform _parentTransform;
        private ShadowCircleFollow _circleAI;
        private bool _isActive = false;
        private Vector2 _possition;
        private Vector2 _defaultOffset = new Vector3(1.02f, 0.14f);
        private float _lastAnimationStarted;
        private Transform _parent;


        private void Start()
        {
            _parentTransform = GetComponentInParent<Transform>();
            _circleAI = this.transform.parent.GetComponentInChildren<ShadowCircleFollow>();
            _lastAnimationStarted = Time.time;
            _parent = transform.parent;
        }

        public void SetActive(Vector2 possition, float rotation)
        {
            if (!_isActive)
            {
                this.transform.position = possition;
                this.transform.rotation = Quaternion.Euler(0, 0, rotation);
                _lastAnimationStarted = Time.time;
                _possition = (Vector2)this.transform.parent.position;
                _animator.Play("GhostAttackPrep");
                _isActive = true;
            }
        }

        private void FixedUpdate()
        {
            if (_isActive)
            {
                if (Time.time > _lastAnimationStarted + 1.2f)
                {
                    _isActive = false;
                }
            }
        }

    }
}
