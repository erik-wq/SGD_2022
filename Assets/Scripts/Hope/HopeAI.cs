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
    [SerializeField] private Transform LightTransform;
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D Light;
    [SerializeField] private float MaxLightScale = 20f;
    [SerializeField] private float MaxLightIntensity = 2.5f;
    [SerializeField] private float MinLightScale = 3f;
    [SerializeField] private float MinLightIntensity = 0.5f;
    [SerializeField][Range(0,100)] private float LowEnergyState = 20;
    [SerializeField] LayerMask ObjectMaks;
    #endregion

    #region Private
    private bool _isFighting = false;
    private IFollow _followScript;
    private List<IShadowEnemy> _attackingEnemies = new List<IShadowEnemy>();
    private int _maxEnemies = 1;
    [SerializeField]private float _hp;
    private float _maxColor = 1;
    private float _lastStunnedTime;
    private HopeLaser _hopeLaser;
    private HopeThrow _hopeThrow;
    private HopeExplosion _hopeExplosion;
    private bool _isMovementLocked = false;
    private bool _isAbilityLocked = false;
    private bool _isHopeLocked = false;
    #endregion

    #region Public
    public LayerMask objectMask 
    {
        get
        {
            return ObjectMaks;
        }
    }
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

    public bool IsHopeLocked
    {
        get
        {
            return _isHopeLocked;
        }

        set
        {
            _isHopeLocked = value;
        }
    }
    public float hp 
    { 
        get 
        { 
            return _hp; 
        } 
    }
    public float LowEnergy
    {
        get
        {
            return LowEnergyState;
        }
    }

    [Header("PathFinding")]
    public BasicFollow folow;
    public Transform target;

    [Header("Idle parameters")]
    public float idleRadius;
    public Transform player;
    [Header("Collecting parameters")]
    public float collectRadius;
    public float CheckRouteRadius;
    public LayerMask enemyMask;
    private HopeStateMachine _machine;
    #endregion


    void Awake()
    {
        _hopeLaser = GetComponent<HopeLaser>();
        _hopeThrow = GetComponent<HopeThrow>();
        _followScript = GetComponent<BasicFollow>();
        _hopeExplosion = GetComponent<HopeExplosion>();
        _hp = MaxHP;
    }
    private void Start()
    {
        folow.SetTarget(target);
        _machine = new HopeStateMachine(this);
    }

    private void FixedUpdate()
    {
        _machine.state.FixedUpdate();
    }

    public void Collect()
    {
        _machine.Collect();
        if (_machine.state.GetType() == typeof(HopeColect))
        {
            HopeColect stat = (HopeColect)_machine.state;
            stat.Collect();
        }
    }

    #region damage
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
    #endregion
    #region ability
    public void OnThrow()
    {
        if (!IsAbilityLocked)
        {
            if (_hp > _hopeThrow.GetCost())
            {
                if (_hopeThrow.Activate())
                {
                    SetHP(_hp - _hopeThrow.GetCost());
                }
            }
        }
    }

    public void OnCancelAction()
    {
        _hopeThrow.CancelAction();
    }

    public void OnFireLaser()
    {
        if (!IsAbilityLocked)
        {
            if (_hp > _hopeLaser.GetCost())
            {
                if (_hopeLaser.Activate())
                {
                    SetHP(_hp - _hopeLaser.GetCost());
                }
            }
        }
    }

    public void OnExplode()
    {
        if (!IsAbilityLocked && !IsHopeLocked)
        {
            if (_hp > _hopeExplosion.GetCost())
            {
                if (_hopeExplosion.Activate())
                {
                    SetHP(_hp - _hopeExplosion.GetCost());
                }
            }
        }
    }

    public void MouseClick()
    {
        _hopeThrow.MouseClick();
    }

    private void SetHP(float hp)
    {
        this._hp = hp;
        AdjustLight();
    }

    private void AdjustLight()
    {
        var lightIntensity = ((_hp / MaxHP) * (MaxLightIntensity - MinLightIntensity)) + MinLightIntensity;
        var lightScale = ((_hp / MaxHP) * (MaxLightScale - MinLightScale)) + MinLightScale;
        Light.intensity = lightIntensity;
        LightTransform.localScale = new Vector3(lightScale, lightScale, 1);
    }

    public void AddEnergy(float ammount)
    {
        var newHP = _hp + ammount;
        if (newHP > MaxHP)
            newHP = MaxHP;
        SetHP(newHP);
    }
    #endregion
    //private void HandleAttack()
    //{
    //    List<IShadowEnemy> toBeRemoved = new List<IShadowEnemy>();

    //    foreach (var item in _attackingEnemies)
    //    {
    //        if (item.TakeDamage(Damage * Time.fixedDeltaTime, 0, this.transform.position))
    //        {
    //            toBeRemoved.Add(item);
    //        }
    //    }

    //    _attackingEnemies = _attackingEnemies.Where(a => !toBeRemoved.Contains(a)).ToList();

    //    if (_attackingEnemies.Count == 0)
    //    {
    //        _isFighting = false;
    //        _followScript.Paused = false;
    //    }
    //}
}
