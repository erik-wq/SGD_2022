using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HopeLowEnergy : BaseState
{
    public HopeLowEnergy(HopeStateMachine machine)
    {
        _machine = machine;
        Start();
    }
    public override void Start()
    {
        _machine.AI.folow.SetTarget(_machine.AI.player);
    }
    public override void FixedUpdate()
    {
        if(_machine.AI.hp > _machine.AI.LowEnergy)
        {
            Exit();
        }
    }
    public override void Exit()
    {
        _machine.Idle();
    }
}
