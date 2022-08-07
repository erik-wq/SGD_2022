using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HopeColect : BaseState
{
    bool _collect;
    public HopeColect(HopeStateMachine machine)
    {
        _machine = machine;
        Collider2D close = Closest();
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
        if (!CheckPath())
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
    private Collider2D Closest()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(_machine.AI.transform.position, _machine.AI.collectRadius, LayerMask.GetMask("Charge"));
        if (cols.Length == 0) return null;
        if (cols.Length == 1)
        {
            return cols[0];
        }
        float dist = Vector2.Distance(_machine.AI.transform.position, cols[0].transform.position);
        Collider2D final = cols[0];
        foreach (Collider2D x in cols)
        {
            if (Vector2.Distance(_machine.AI.transform.position, x.transform.position) < dist)
            {
                final = x;
            }
        }
        return final;
    }
    private bool CheckPath()
    {
        List<Vector3> path = _machine.AI.folow.path.vectorPath;
        Physics2D.queriesHitTriggers = false;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(path[i], _machine.AI.CheckRouteRadius , _machine.AI.enemyMask);
            if (cols.Length != 0)
            {
                Physics2D.queriesHitTriggers = true;
                return false;
            }
        }
        Physics2D.queriesHitTriggers = true;
        return true;
    }
}
