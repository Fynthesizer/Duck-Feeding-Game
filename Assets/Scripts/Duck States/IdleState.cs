using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : DuckState
{
    public override DuckStateID GetID()
    {
        return DuckStateID.Idle;
    }

    private float idleTimer = 0f;
    private float lookTimer = 0f;

    public IdleState(Duck duck) : base(duck)
    {

    }

    public override void Enter()
    {
        idleTimer = Random.Range(duck.globalVars.minIdleTime, duck.globalVars.maxIdleTime);
        lookTimer = Random.Range(0.5f, 2.5f);
    }

    public override void Update()
    {
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0f)
        {
            if (Random.value < 0.9f) duck.stateMachine.ChangeState(DuckStateID.Wander);
            else duck.stateMachine.ChangeState(DuckStateID.Preen);
        }

        lookTimer -= Time.deltaTime;
        if (lookTimer <= 0f)
        {
            lookTimer = Random.Range(0.5f, 2.5f);
            ChooseNewLookTarget();
        }
    }

    private void ChooseNewLookTarget()
    {
        float angle = Random.Range(-45f, 45f);
        duck.targetLookDirection = Quaternion.AngleAxis(angle, Vector3.up) * -duck.transform.forward;
    }

    public override void UpdateNearestFood(GameObject food)
    {
        if (food != null) duck.stateMachine.ChangeState(DuckStateID.Pursuit);
    }
}
