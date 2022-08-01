using Assets.TestingAssets;
using Assets.TestingAssets.TestScripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HopeAI : MonoBehaviour
{
    #region Serialized
    [SerializeField] private float Damage = 10;
    [SerializeField] private float MaxHP = 100;
    [SerializeField] private float StunnedLenght = 1.3f;
    [SerializeField] private SpriteRenderer MainSprite;
    #endregion

    #region Private
    private bool _isFighting = false;
    private IFollow _followScript;
    private List<IShadowEnemy> _attackingEnemies = new List<IShadowEnemy>();
    private int _maxEnemies = 1;
    private float _hp;
    private float _maxColor = 1;
    private float _lastStunnedTime;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _hp = MaxHP;
        _followScript = GetComponent<IFollow>();
    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void FixedUpdate()
    {
        if (_isFighting)
        {
            HandleAttack();
        }
    }

    private void HandleAttack()
    {
        List<IShadowEnemy> toBeRemoved = new List<IShadowEnemy>();

        foreach (var item in _attackingEnemies)
        {
            if (item.TakeDamage(Damage * Time.fixedDeltaTime, 0, this.transform.position))
            {
                toBeRemoved.Add(item);
            }
        }

        _attackingEnemies = _attackingEnemies.Where(a => !toBeRemoved.Contains(a)).ToList();

        if (_attackingEnemies.Count == 0)
        {
            _isFighting = false;
            _followScript.Paused = false;
        }
    }

    private void TakeDamage(float damage)
    {
        _hp -= damage;
        AdjustColor();
    }

    private void AdjustColor()
    {
        float percentageHP = _hp / MaxHP;
        float colorFactor = _maxColor * percentageHP;
        MainSprite.color = new Color(colorFactor, colorFactor, colorFactor, MainSprite.color.a);
    }

    public bool GetAttacked(IShadowEnemy enemy)
    {
        if ((!_isFighting || _attackingEnemies.Count < _maxEnemies) && Time.time > _lastStunnedTime + StunnedLenght)
        {
            _isFighting = true;
            if (!_attackingEnemies.Contains(enemy))
                _attackingEnemies.Add(enemy);
            _followScript.Paused = true;

            return true;
        }
        else
        {
            _attackingEnemies.ForEach(a => a.SetFree());
            _isFighting = false;
            _followScript.Paused = false;
            _isFighting = false;
            _lastStunnedTime = Time.time;
            TakeDamage(enemy.Damage);

            return false;
        }
    }
}
