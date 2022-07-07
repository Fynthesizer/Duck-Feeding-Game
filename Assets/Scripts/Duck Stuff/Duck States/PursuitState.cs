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

    public override bool allowQuack { get { return false; } }

    private Vector3 targetPosition;
    private GameObject targetObject;

    public PursuitState(Duck duck) : base(duck)
    {
        HeadIkWeight = 0f;
        NeckRotationWeight = 1f;
        BillOpenness = 0f;
        AnimatorWeight = 0f;
    }

    public override void Enter()
    {
        base.Enter();

        thrustTimer = duck.globalVars.thrustInterval;
        UpdateTarget();
    }

    public override void Update()
    {
        if (!active) return;

        Vector3 targetDirection = (targetPosition - duck.transform.position).normalized;
        duck.targetLookDirection = -targetDirection;

        float targetDistance = Vector3.Distance(duck.head.position, targetPosition);
        float targetDot = Vector3.Dot(duck.transform.forward, (targetPosition - duck.transform.position).normalized);
        float speedMultiplier = 1f;

        if (targetDistance < 1f && targetDot > 0f)
        {
            speedMultiplier = Utilities.Map(targetDistance, 1f, 0f, 1f, 0.5f);
            float foodCloseness = (1f - targetDistance);

            duck.headIK.data.target.position = targetPosition + new Vector3(0f, 0.03f, 0f);
            HeadIkWeight = foodCloseness;
            NeckRotationWeight = 0f;
            BillOpenness = foodCloseness;
        }
        else
        {
            HeadIkWeight = 0f;
            BillOpenness = 0f;
            NeckRotationWeight = 1f;
        }

        Swim(speedMultiplier);

        if (targetDistance < 0.1f && targetObject != null)
        {
            duck.Eat(targetObject);
            duck.stateMachine.ChangeState(DuckStateID.Eat);
        }
    }

    private void Swim(float speedMultiplier)
    {
        thrustTimer -= Time.deltaTime;

        if (thrustTimer <= 0f)
        {
            thrustTimer = duck.globalVars.thrustInterval;
            duck.Swim(targetPosition, duck.speed * duck.globalVars.pursuitSpeedMultiplier * speedMultiplier, duck.globalVars.pursuitAvoidLayers);
        }
    }

    private void UpdateTarget()
    {
        targetObject = duck.nearestFood;
        targetPosition = targetObject.transform.position;
    }

    public override void Exit()
    {

    }

    public override void UpdateNearestFood(GameObject food)
    {
        if (food == null) duck.stateMachine.ChangeState(DuckStateID.Idle);
        else if (food != targetObject) UpdateTarget();
    }
}
