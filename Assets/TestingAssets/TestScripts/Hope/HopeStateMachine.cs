using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HopeStateMachine
{
    public BaseState state { get; private set; }
    public HopeAI AI { get; private set; }
    public HopeStateMachine(HopeAI ai)
    {
        this.AI = ai;
        this.state = new HopeIdle(this);
    }
    public bool CheckCharge()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(AI.transform.position, AI.collectRadius, LayerMask.GetMask("Charge"));
        if (cols.Length != 0)
        {
            Collider2D col = Closest(cols);

            if (Vector2.Distance(col.transform.position, AI.transform.position) > 40)
                return false;

            AI.folow.SetTarget(col.transform);
            if (CheckPath())
            {
                return true;
            }
            return false;
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
    public void LowEnergy()
    {
        this.state = null;
        state = new HopeLowEnergy(this);
    }
    public void CheckEnergy()
    {
        if(AI.hp < AI.LowEnergy)
        {
            LowEnergy();
        }
    }
    private Collider2D Closest(Collider2D[] cols)
    {
        if (cols.Length == 1)
        {
            return cols[0];
        }
        float dist = Vector2.Distance(AI.transform.position, cols[0].transform.position);
        Collider2D final = cols[0];
        foreach (Collider2D x in cols)
        {
            if (Vector2.Distance(AI.transform.position, x.transform.position) < dist)
            {
                final = x;
            }
        }

        return final;
    }
    public Collider2D Closest()
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(AI.transform.position, AI.collectRadius, LayerMask.GetMask("Charge"));
        if (cols.Length <= 0) return null;
        if (cols.Length == 1)
        {
            return cols[0];
        }
        float dist = Vector2.Distance(AI.transform.position, cols[0].transform.position);
        Collider2D final = cols[0];
        foreach (Collider2D x in cols)
        {
            if (Vector2.Distance(AI.transform.position, x.transform.position) < dist)
            {
                final = x;
            }
        }
        return final;
    }
    public bool CheckPath()
    {
        List<Vector3> path = AI.folow.path.vectorPath;
        Physics2D.queriesHitTriggers = false;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Collider2D[] cols = Physics2D.OverlapCircleAll(path[i], AI.CheckRouteRadius, AI.enemyMask);
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
