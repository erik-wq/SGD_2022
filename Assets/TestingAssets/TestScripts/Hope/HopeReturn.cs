using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HopeReturn : BaseState
{
    public HopeReturn(HopeStateMachine machine)
    {
        _machine = machine;
        _machine.AI.folow.SetTarget(_machine.AI.player);
    }
    public override void FixedUpdate()
    {
        if(Vector2.Distance(_machine.AI.transform.position, _machine.AI.player.position) < _machine.AI.idleRadius)
        {
            Exit();
            return;
        }
        _machine.AI.folow.SetTarget(_machine.AI.player);
    }
    public override void Exit()
    {
        _machine.Idle();
    }
}
