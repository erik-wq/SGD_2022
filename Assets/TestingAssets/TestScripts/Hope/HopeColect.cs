using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HopeColect : BaseState
{
    bool _collect;
    public HopeColect(HopeStateMachine machine)
    {
        _machine = machine;
        Collider2D close = _machine.Closest();
        if(close == null)
        {
            Exit();
            return;
        }
        _machine.AI.folow.SetTarget(close.transform);
        _collect = false;
    }
    public override void FixedUpdate()
    {
        if (!_machine.CheckPath())
        {
            Exit();
            return;
        }
        List<Vector3> path = _machine.AI.folow.path.vectorPath;
        if (Vector2.Distance(_machine.AI.transform.position,path[path.Count - 1]) < (_machine.AI.folow.minDistance + 0.25f) && _collect)
        {
            Exit();
            return;
        }

        if (Vector2.Distance(_machine.AI.transform.position, path[path.Count - 1]) > _machine.AI.collectRadius)
        {
            Exit();
            return;
        }

        if(_machine.AI.folow.Target == null)
        {
            Exit();
            return;
        }
    }
    public override void Exit()
    {
        _machine.Return();
    }
    public void Collect()
    {
        _collect = true;
    }
}
