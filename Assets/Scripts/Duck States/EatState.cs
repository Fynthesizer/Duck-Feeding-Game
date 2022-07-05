using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatState : DuckState
{
    private float eatTimer = 0f;

    public override DuckStateID GetID()
    {
        return DuckStateID.Eat;
    }
    public override bool allowQuack { get { return false; } }
    public override bool allowLook { get { return false; } }

    public EatState(Duck duck) : base(duck)
    {

    }

    public override void Enter()
    {
        eatTimer = duck.globalVars.eatTime;
        duck.animator.SetTrigger("Eat");
        //yield return new WaitForSeconds(duck.globalVars.eatTime);
        //if (duck.state == this) duck.SetState(new WanderState(duck));
    }

    public override void Update()
    {
        eatTimer -= Time.deltaTime;
        if (eatTimer < 0f) duck.stateMachine.ChangeState(DuckStateID.Idle);
    }

    public override void UpdateNearestFood(GameObject food)
    {
        return;
    }
}
