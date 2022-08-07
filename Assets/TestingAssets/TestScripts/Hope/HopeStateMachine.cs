using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HopeStateMachine
{
    public BaseState state { get; private set; }
    public HopeAi AI { get; private set; }
    public HopeStateMachine(HopeAi ai)
    {
        this.AI = ai;
        this.state = new HopeIdle(this);
    }
    public bool CheckCharge()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(AI.transform.position, AI.collectRadius, LayerMask.GetMask("Charge"));
        if (cols.Length != 0)
        {
            return true;
        }
        return false;
    }
    public void Idle()
    {
        this.state = null;
        this.state = new HopeIdle(this);
    }
    public void Collect()
    {
        this.state = null;
        this.state = new HopeColect(this);
    }
    public void Return()
    {
        this.state = null;
        state = new HopeReturn(this);
    }
}
