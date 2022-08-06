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
            AI.folow.SetTarget(Closest(cols).transform);
            bool check = CheckPath();
            if (check)
            {
                state.Exit();
                return true;
            }
            //Debug.Log("checked");
        }
        AI.folow.SetTarget(AI.target);
        return false;
    }
    private Collider2D Closest(Collider2D[] cols)
    {
        if(cols.Length == 1)
        {
            return cols[0];
        }
        float dist = Vector2.Distance(AI.transform.position, cols[0].transform.position);
        Collider2D final = cols[0];
        foreach(Collider2D x in cols)
        {
            if(Vector2.Distance(AI.transform.position, x.transform.position) < dist)
            {
                final = x;
            }
        }
        return final;
    }
    public bool CheckPath()
    {
        Vector2 dir;
        float dist;
        List<Vector3> path = AI.folow.path.vectorPath;
        for(int i =0; i < path.Count -2; i += 2)
        {
            dir = (path[i + 2] - path[i]).normalized;
            dist = Vector2.Distance(path[i], path[i + 2]);
            if(Physics2D.BoxCast(path[i],new Vector2(dist, AI.CheckRouteSize.y), 0, dir,dist,LayerMask.GetMask("Enemy")))
            {
                return false;
            }
        }
        dir = (path[path.Count -3] - path[path.Count -1]).normalized;
        dist = Vector2.Distance((Vector2)path[path.Count - 3] , (Vector2)path[path.Count - 1]);
        if (Physics2D.BoxCast(path[path.Count -3], new Vector2(dist, AI.CheckRouteSize.y), 0, dir, dist, LayerMask.GetMask("Enemy")))
        {
            return false;
        }
        return true;
    }
    public void Idle()
    {
        //Debug.Log("idle");
        this.state = new HopeIdle(this);
    }
    public void Collect()
    {
        //Debug.Log("Collesct");

        this.state = new HopeColect(this);
    }
    public void Return()
    {
        //Debug.Log("return");

        state = new HopeReturn(this);
    }
}
