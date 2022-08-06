using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HopeColect : BaseState
{
    bool _collect;
    public HopeColect(HopeStateMachine machine)
    {
        _machine = machine;
        _collect = false;
    }
    public override void FixedUpdate()
    {
        if (!_machine.CheckPath())
        {
            //Debug.Log("Exit path");
            Exit();
            return;
        }
        List<Vector3> path = _machine.AI.folow.path.vectorPath;
        if (Vector2.Distance(_machine.AI.transform.position,path[path.Count - 1]) < (_machine.AI.folow.minDistance + 0.25f) && _collect)
        {
            //Debug.Log("Exit collected");
            Exit();
            return;
        }
        if (Vector2.Distance(_machine.AI.transform.position, path[path.Count - 1]) > _machine.AI.collectRadius)
        {
            //Debug.Log("Exit collect radius distance");

            Exit();
            return;
        }
        if(_machine.AI.folow.Target == null)
        {
            //Debug.Log("Exit collect null");

            Exit();
            return;
        }
    }
    public override void Exit()
    {
        //Debug.Log("Exit collect");
        _machine.Return();
    }
    public void Collect()
    {
        //Debug.Log("collected energy");
        _collect = true;
    }
}
