using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Environment
{
    class GateControl : MonoBehaviour
    {
        [SerializeField] private Sprite OpenedSprite;

        private SpriteRenderer _sprite;
        private bool _isActive = false;
        private BoxCollider2D _collider;

        private void Start()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();
        }

        public void Open()
        {
            //_sprite.sprite = OpenedSprite;
            _isActive = true;
            _collider.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.gameObject.tag == "Player")
            {
                if (_isActive)
                {
                    var playerTransform = Global.Instance.PlayerTransform;
                    var hopeTransform = Global.Instance.HopeTransform;

                    playerTransform.position = new Vector3(39, 199, 0);
                    hopeTransform.position = new Vector3(39, 198, 0);
                }
            }
        }
    }
}
