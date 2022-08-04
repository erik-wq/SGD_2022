using Assets.Scripts.Hope;
using Assets.TestingAssets;
using Assets.TestingAssets.TestScripts;
using Assets.TestingAssets.TestScripts.Hope;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private HopeLaser _hopeLaser;
    private HopeThrow _hopeThrow;
    private bool _isMovementLocked = false;
    private bool _isAbilityLocked = false;
    #endregion

    #region Public
    public bool IsMovementLocked
    {
        get
        {
            return _isMovementLocked;
        }

        set
        {
            _followScript.Paused = value;
            _isMovementLocked = value;
        }
    }

    public bool IsAbilityLocked
    {
        get
        {
            return _isAbilityLocked;
        }

        set
        {
            _isAbilityLocked = value;
        }
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _hp = MaxHP;
        _followScript = GetComponent<IFollow>();
        _hopeLaser = GetComponent<HopeLaser>();
        _hopeThrow = GetComponent<HopeThrow>();

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

    public void OnThrow()
    {
        if (!IsAbilityLocked)
            _hopeThrow.Activate();
    }

    public void OnCancelAction()
    {
        _hopeThrow.CancelAction();
    }

    public void OnFireLaser()
    {
        if (!IsAbilityLocked)
            _hopeLaser.Activate();
    }

    public void MouseClick()
    {
        _hopeThrow.MouseClick();
    }
}
