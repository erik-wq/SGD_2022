using Assets.Scripts;
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
        _machine.AI.follow.SetTarget(close.transform);
        _collect = false;
    }
    public override void FixedUpdate()
    {
        if (!_machine.CheckPath())
        {
            Exit();
            return;
        }

        List<Vector3> path = _machine.AI.follow.path.vectorPath;
        if (Vector2.Distance(_machine.AI.transform.position,path[path.Count - 1]) < (_machine.AI.follow.minDistance + 0.25f) && _collect)
        {
            Exit();
            return;
        }
        if (Vector2.Distance(_machine.AI.player.transform.position, path[path.Count - 1]) > _machine.AI.collectRadius)
        {
            Exit();
            return;
        }

        if(_machine.AI.follow.Target == null)
        {
            Exit();
            return;
        }

        if(PathLength() > 35)
        {
            Exit();
            return;
        }
    }

    private float PathLength()
    {
        return Vector2.Distance(Global.Instance.HopeTransform.position, _machine.AI.follow.path.vectorPath[_machine.AI.follow.path.vectorPath.Count - 1]);
    }
    public override void Exit()
    {
        _machine.Idle();
    }
    public void Collect()
    {
        _collect = true;
    }
}
