using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.TestingAssets.TestScripts.Hope;
using Assets.Scripts.Hope;

public class HopeAi : MonoBehaviour
{
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
    private bool _isMovementLocked = false;
    private bool _isAbilityLocked = false;
    private HopeLaser _hopeLaser;
    private HopeThrow _hopeThrow;


    public bool IsMovementLocked
    {
        get
        {
            return _isMovementLocked;
        }

        set
        {
            folow.Paused = value;
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
    private void Awake()
    {
        _hopeLaser = GetComponent<HopeLaser>();
        _hopeThrow = GetComponent<HopeThrow>();

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
        Debug.Log(_machine.state.GetType());
        if(_machine.state.GetType() == typeof(HopeColect))
        {
            HopeColect stat = (HopeColect)_machine.state;
            stat.Collect();
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
