using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DuckStateID
{
    Idle,
    Wander,
    Pursuit,
    Preen
}

public abstract class DuckState
{
    public virtual DuckStateID GetID() {
        return DuckStateID.Idle;
    }

    protected Duck duck;
    public virtual bool allowQuack { get { return true; } }
    public virtual bool allowLook { get { return true; } }

    public DuckState(Duck _duck)
    {
        duck = _duck;
    }

    public virtual void Enter()
    {
        
    }


    public virtual void Exit()
    {
        
    }

    public virtual void Swim()
    {
    }

    public virtual void Update()
    {

    }


    public virtual void UpdateNearestFood(GameObject nearest)
    {
    }
}


