using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseState
{
    protected HopeStateMachine _machine;
    public virtual void Start() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
}
