using Assets.Scripts.Utils;
using Assets.TestingAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Totems
{
    class TotemAI : MonoBehaviour, IEnemy
    {
        #region Serialized
        [SerializeField] Transform PlayerTransform;
        [SerializeField] Transform HopeTransform;
        [SerializeField] PlayerController PlayerControl;
        [SerializeField] HopeAI HopeAIScript;
        [SerializeField] private GameObject MeleePrefab;
        [SerializeField] private GameObject GhostPrefab;
        [SerializeField] private GameObject SpawnerPrefab;

        [SerializeField] private float MaxHP = 1000;
        [SerializeField] private int NumberOfStages = 4;
        [SerializeField] private float SpawnTick = 10;
        [SerializeField] private float SpawnTickDecrese = 1;
        [SerializeField] private int SpawnRateMelee = 2;
        [SerializeField] private float SpawnRateMeleeIncrease = 1;
        [SerializeField] private int SpawnRateGhost = 0;
        [SerializeField] private float SpawnRateGhostIncrease = 0;
        [SerializeField] private CircleCollider2D SpawnCircle;
        [SerializeField] private string Name;
        [SerializeField] private TMP_Text NameText;
        [SerializeField] private Image HealImage;

        [SerializeField] private int MaxMelee = 10;
        [SerializeField] private int MaxGhosts = 3;

        [SerializeField] private Animator _animator;
        #endregion

        #region Private
        private float _lastTick = 0;
        private bool _isActive = true;
        private float _currentHP;
        private int _currentStage = 0;
        private bool _isRunning = false;
        private bool _hasBeenDestroyed = false;
        private event Action _onDestroyed;
        private int _meleeCount = 0;
        private int _shadowCount = 0;
        #endregion
        private void Start()
        {
            _currentHP = MaxHP;

            if (HopeAIScript == null || HopeTransform == null || PlayerTransform == null || PlayerControl == null)
            {
                HopeAIScript = Global.Instance.HopeScript;
                HopeTransform = Global.Instance.HopeTransform;
                PlayerControl = Global.Instance.PlayerScript;
                PlayerTransform = Global.Instance.PlayerTransform;
            }
        }
        private void FixedUpdate()
        {
            if (_isRunning)
            {
                if (Time.time > _lastTick + (SpawnTick - SpawnTickDecrese * _currentStage))
                {
                    Spawn();
                }
            }
        }

        private void Die()
        {
            _isRunning = false;
            NameText.enabled = false;
            HealImage.enabled = false;
            _hasBeenDestroyed = true;

            if(_onDestroyed != null)
            {
                _onDestroyed();
            }
        }

        public void RegisterOnDestroy(Action action)
        {
            _onDestroyed += action;
        }

        private void Spawn()
        {
            int meleeCount = Mathf.RoundToInt(SpawnRateMelee + _currentStage * SpawnRateMeleeIncrease);
            int ghostCount = Mathf.RoundToInt(SpawnRateGhost + _currentStage * SpawnRateGhostIncrease);

            for (int i = 0; i < meleeCount; i++)
            {
                if (_meleeCount >= MaxMelee)
                    break;
                SpawnMelee(FindFreePoint());
            }

            for (int i = 0; i < ghostCount; i++)
            {
                if (_shadowCount >= MaxGhosts)
                    break;
                SpawnGhost(FindAnyPoint());
            }

            _lastTick = Time.time;
        }

        private void SpawnMelee(Vector2 possition)
        {
            var obj = Instantiate(SpawnerPrefab, possition, Quaternion.identity);
            var script = obj.GetComponent<TotemSpawner>();
            script.RegisterOnSpawned(OnSpawnedMelee);
            _meleeCount++;

        }

        private void OnSpawnedMelee(GameObject obj)
        {
            obj.GetComponent<EnemyAI>().RegisterOnDeath(OnDeathMelee);
        }

        private void SpawnGhost(Vector2 possition)
        {
            var obj = Instantiate(GhostPrefab, possition, Quaternion.identity);
            obj.GetComponent<BasicShadowAI>().RegisterOnDeath(OnDeathMelee);
            _shadowCount++;
        }

        public void ClearForces()
        {
        }

        private void OnDeathMelee()
        {
            _meleeCount--;
        }

        private void OnDeathGhost()
        {
            _shadowCount--;
        }

        private Vector2 FindFreePoint()
        {
            Vector2 freePoint = Vector2.zero;
            int i = 0;
            while (freePoint == Vector2.zero)
            {
                if (i > 1000)
                    return Vector2.zero;

                var possition = FindAnyPoint();
                var collisions = Physics2D.OverlapBoxAll(possition, new Vector2(1, 1), 0).ToList();
                collisions = RealCollisions(collisions);
                if (collisions.Count() == 0)
                {
                    return possition;
                }

                i++;
            }

            return Vector2.zero;
        }

        private List<Collider2D> RealCollisions(List<Collider2D> collisions)
        {
            return collisions.Where(a => a.gameObject.tag == "CollisionObject").ToList();
        }

        private Vector2 FindAnyPoint()
        {
            var radius = UnityEngine.Random.Range(0, SpawnCircle.radius);
            var angle = UnityEngine.Random.Range(0, 360);
            var direction = MathUtility.RotateVector(Vector2.up, angle);
            var relativePoint = direction * radius;
            return (Vector2)SpawnCircle.transform.position + relativePoint;
        }

        private void StartEncounter()
        {
            _isRunning = true;
        }

        private void AdjustHP()
        {
            HealImage.fillAmount = _currentHP / MaxHP;
        }

        public bool TakeDamage(float damage, float force, Vector2 origin)
        {
            this.TakeDamage(damage);
            return true;
        }

        public bool TakeDamage(float damage)
        {
            _currentHP -= damage;
            if (_currentHP <= 0)
            {
                Die();
                _animator.SetBool("IsDead", true);
            }
            else
            {
                AdjustHP();
            }

            return true;
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.gameObject.tag == "Player" && !_hasBeenDestroyed)
            {
                StartEncounter();
                NameText.text = Name;
                AdjustHP();
                NameText.enabled = true;
                HealImage.enabled = true;
            }
        }

        public void PauseFollow()
        {
            throw new NotImplementedException();
        }

        public void UnPauseFollow()
        {
            throw new NotImplementedException();
        }
    }
}
