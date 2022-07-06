using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PursuitState : DuckState
{
    private float thrustTimer = 0f;

    public override DuckStateID GetID()
    {
        return DuckStateID.Pursuit;
    }

    private Vector3 targetPosition;
    private GameObject targetObject;

    public PursuitState(Duck duck) : base(duck)
    {

    }

    public override void Enter()
    {
        thrustTimer = 0.2f;
        UpdateTarget();
        //else duck.SetState(new IdleState(duck));

        duck.headIKWeight = 0f;
        duck.neckRotationWeight = 1f;
        duck.animatorWeight = 1f;
    }

    public override void Update()
    {
        Swim();

        Vector3 targetDirection = (targetPosition - duck.transform.position).normalized;
        duck.targetLookDirection = -targetDirection;

        float targetDistance = Vector3.Distance(duck.transform.position, targetPosition);
        float targetDot = Vector3.Dot(duck.transform.forward, (targetPosition - duck.transform.position).normalized);

        if (targetDistance < 1f && targetDot > 0.6f)
        {
            duck.headIK.transform.GetChild(0).position = targetPosition;
            duck.headIKWeight = (1f - targetDistance);
            duck.neckRotationWeight = 0f;
        }
        else
        {
            duck.headIKWeight = 0f;
            duck.neckRotationWeight = 1f;
        }

        if (targetDistance < 0.3f)
        {
            duck.Eat(targetObject);
            duck.stateMachine.ChangeState(DuckStateID.Eat);
        }
    }

    private void Swim()
    {
        thrustTimer -= Time.deltaTime;

        if (thrustTimer <= 0f)
        {
            thrustTimer = 0.2f;
            duck.Swim(targetPosition, duck.speed * duck.globalVars.pursuitSpeedMultiplier, duck.globalVars.pursuitAvoidLayers);
        }
    }

    private void UpdateTarget()
    {
        targetObject = duck.nearestFood;
        targetPosition = targetObject.transform.position;
    }

    public override void Exit()
    {
        duck.headIKWeight = 0f;
    }

    public override void UpdateNearestFood(GameObject food)
    {
        if (food == null) duck.stateMachine.ChangeState(DuckStateID.Idle);
        else if (food != targetObject) UpdateTarget();
    }
}
