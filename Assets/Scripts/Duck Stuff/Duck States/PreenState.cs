using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreenState : DuckState
{
    public override DuckStateID GetID()
    {
        return DuckStateID.Preen;
    }

    private float preenTimer = 0f;


    public override bool allowQuack { get { return false; } }
    public override bool allowLook { get { return false; } }
    public PreenState(Duck duck) : base(duck)
    {

    }

    public override void Enter()
    {
        preenTimer = Random.Range(duck.globalVars.minIdleTime, duck.globalVars.maxIdleTime);
        duck.animator.SetBool("Mirror", Random.value > 0.5f);
        duck.animator.SetBool("Preening", true);

        duck.animatorWeight = 1f;
        duck.headIKWeight = 0f;
        duck.neckRotationWeight = 0f;
    }

    public override void Update()
    {
        preenTimer -= Time.deltaTime;

        if (preenTimer <= 0f)
        {
            if (Random.value > 0.5f) duck.stateMachine.ChangeState(DuckStateID.Wander);
            else duck.stateMachine.ChangeState(DuckStateID.Idle);
        }
    }

    public override void Exit()
    {
        duck.animator.SetBool("Preening", false);
        //return base.Exit();
    }

    /*
    public override void UpdateNearestFood(GameObject nearest)
    {
        if (nearest != null && nearest.activeInHierarchy) duck
    }
    */
}
