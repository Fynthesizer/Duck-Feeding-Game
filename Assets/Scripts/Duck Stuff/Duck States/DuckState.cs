using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DuckStateID
{
    Idle,
    Wander,
    Pursuit,
    Eat,
    Preen
}

public abstract class DuckState
{
    public virtual DuckStateID ID { get { return DuckStateID.Idle; } }

    public virtual DuckStateID GetID() {
        return DuckStateID.Idle;
    }

    protected Duck duck;
    public virtual bool allowQuack { get { return true; } }
    public virtual bool allowLook { get { return true; } }
    public bool active = false;

    //Animation weights
    public float HeadIkWeight = 0f;
    public float AnimatorWeight = 0f;
    public float NeckRotationWeight = 0f;
    public float BillOpenness = 0f;

    public float QuackFrequency = 1f;

    public DuckState(Duck _duck)
    {
        duck = _duck;
    }

    public virtual void Enter()
    {
        active = true;
    }

    public virtual void Exit()
    {
        active = false;
    }


    public virtual void Update()
    {
        
    }


    public virtual void UpdateNearestFood(GameObject food)
    {
        if (food != null) duck.stateMachine.ChangeState(DuckStateID.Pursuit);
    }
}


