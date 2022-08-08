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
        _machine.AI.folow.SetTarget(_machine.AI.target);
        Vector2 pos = RandomPos();
        _machine.AI.target.position = pos;
    }
    public override void FixedUpdate()
    {
        if (_machine.CheckCharge())
        {
            Exit();
            return;
        }
        if (_machine.AI.folow.path == null) return;
        List<Vector3> path = _machine.AI.folow.path.vectorPath;
        if (Vector2.Distance(_machine.AI.transform.position, path[path.Count - 1]) < 1.1f)
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
    #endregion
}
