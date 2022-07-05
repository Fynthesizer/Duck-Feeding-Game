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

    private float headIKWeight;

    public PursuitState(Duck duck) : base(duck)
    {

    }

    public override void Enter()
    {
        thrustTimer = 0.2f;
        UpdateTarget();
        //else duck.SetState(new IdleState(duck));

        headIKWeight = 0.0f;
    }

    public override void Update()
    {
        Swim();

        Vector3 targetDirection = (targetPosition - duck.transform.position).normalized;
        duck.targetLookDirection = -targetDirection;

        duck.headIK.weight = Mathf.Lerp(duck.headIK.weight, headIKWeight, Time.deltaTime * 5f);
        duck.lookConstraint.weight = Mathf.Lerp(duck.lookConstraint.weight, 1 - headIKWeight, Time.deltaTime * 5f);

        //if (duck.nearestFood == null) duck.stateMachine.ChangeState(DuckStateID.Idle);
        //else if (duck.nearestFood != targetObject) UpdateTarget();

        float targetDistance = Vector3.Distance(duck.transform.position, targetPosition);
        float targetDot = Vector3.Dot(duck.transform.forward, (targetPosition - duck.transform.position).normalized);

        if (targetDistance < 0.3f)
        {
            duck.Eat(targetObject);
            duck.stateMachine.ChangeState(DuckStateID.Eat);
        }

        if (targetDistance < 1f && targetDot > 0.6f)
        {
            duck.headIK.transform.GetChild(0).position = targetPosition;
            headIKWeight = (1f - targetDistance);
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
        headIKWeight = 0f;
        duck.lookConstraint.weight = 1f;
        duck.headIK.weight = 0f;
    }

    public override void UpdateNearestFood(GameObject food)
    {
        if (food == null) duck.stateMachine.ChangeState(DuckStateID.Idle);
        else if (food != targetObject) UpdateTarget();
    }
}
