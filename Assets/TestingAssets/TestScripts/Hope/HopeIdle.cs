using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HopeIdle : BaseState
{
    private int _count;
    private int _range;
    public HopeIdle(HopeStateMachine machine)
    {
        this._machine = machine;
        _count = 0;
        _range = 0;
        Start();
    }
    public override void Start()
    {
        _machine.AI.follow.SetTarget(_machine.AI.target);
        _machine.AI.target.position = RandomPos();
    }
    public override void FixedUpdate()
    {
        if (_machine.AI.IsAbilityLocked)
        {
            return;
        }
        _machine.CheckEnergy();
        if (_machine.CheckCharge())
        {
            Exit();
            return;
        }
        if (CheckPosition())
        {
            Vector2 dest = RandomPos();
            if (dest == Vector2.zero)
            {
                _machine.AI.follow.SetTarget(null);
            }
            else
            {
                _machine.AI.target.position = dest;
                _machine.AI.follow.SetTarget(_machine.AI.target);
            }
        }
        if (_machine.AI.follow.path == null) return;
        List<Vector3> path = _machine.AI.follow.path.vectorPath;
        if (Vector2.Distance(_machine.AI.transform.position, path[path.Count - 1]) < 1.4f)
        {
            Count();
        }
    }
    public override void Exit()
    {
        _machine.Collect();
    }
    #region postions
    private Vector2 RandomPos()
    {
        float radius = _machine.AI.idleRadius;
        float x = _machine.AI.player.position.x + Random.Range(-radius, radius);
        float y = _machine.AI.player.position.y + Random.Range(-radius, radius);
        Vector2 dest = new Vector2(x, y);
        if (Vector2.Distance(_machine.AI.transform.position, dest) < 0.85f)
        {
            return Vector2.zero;
        }
        _count = 0;
        return dest;
    }
    private void Count()
    {
        if (_range == 0)
        {
            _range = Random.Range(8, 25);
        }
        _count++;
        if (_count >= _range)
        {
            Vector2 dest = RandomPos();
            if (dest == Vector2.zero)
            {
                return;
            }
            _machine.AI.target.position = dest;
            _range = 0;
        }
    }
    private bool CheckPosition()
    {
        if (Physics2D.OverlapCircle(_machine.AI.target.position, 0.75f, _machine.AI.objectMask))
        {
            return true;
        }
        return false;
    }
    #endregion
}
