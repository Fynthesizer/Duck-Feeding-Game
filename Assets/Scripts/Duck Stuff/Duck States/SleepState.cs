using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SleepState : DuckState
{
    public override DuckStateID GetID()
    {
        return DuckStateID.Sleep;
    }

    public SleepState(Duck duck) : base(duck)
    {
        animationDriver = AnimationDriver.Animator;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {

    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void UpdateNearestFood(GameObject food)
    {
        if (food != null) duck.stateMachine.ChangeState(DuckStateID.Pursuit);
    }
}
