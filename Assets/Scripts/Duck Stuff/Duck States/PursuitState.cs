using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PursuitState : DuckState
{
    private float thrustTimer = 0f;

    const float reachDistance = 0.5f;

    public override DuckStateID GetID()
    {
        return DuckStateID.Pursuit;
    }

    public override bool allowQuack { get { return false; } }

    private Vector3 targetPosition;
    private GameObject targetObject;

    private bool isReaching = false;

    public PursuitState(Duck duck) : base(duck)
    {
        animationDriver = AnimationDriver.Twist;

        QuackFrequency = 3f;
    }

    public override void Enter()
    {
        base.Enter();

        animationDriver = AnimationDriver.Twist;
        isReaching = false;

        thrustTimer = duck.globalVars.thrustInterval;
        UpdateTarget();
    }

    public override void Update()
    {
        if (!active) return;

        Vector3 targetDirection = (targetPosition - duck.transform.position).normalized;
        duck.targetLookDirection = -targetDirection;

        float targetDistance = Vector3.Distance(duck.mouth.position, targetPosition);
        float targetDot = Vector3.Dot(duck.transform.forward, (targetPosition - duck.transform.position).normalized);
        float speedMultiplier = 1f;

        if (targetDistance < reachDistance)
        {
            if (targetDistance > 0.18f) speedMultiplier = Utilities.Map(targetDistance, 0.5f, 0.18f, 1f, 0.2f);
            else speedMultiplier = 0f;

            if (targetDot > 0.8f && !isReaching)
            {
                BeginReach();
            }
            else if (targetDot < 0.8f && isReaching) EndReach();
        }
        else if (targetDistance > reachDistance && isReaching) EndReach();

        if (isReaching)
        {
            float foodCloseness = Utilities.Map(targetDistance, reachDistance, 0.2f, 0f, 1f);

            duck.headIK.data.target.position = targetPosition + new Vector3(0f, 0.03f, 0f);
            duck.dipAmount = foodCloseness;
            duck.billOpenness = foodCloseness;
        }
        else
        {
            duck.dipAmount = 0f;
            duck.billOpenness = 0f;
        }

        Swim(speedMultiplier);

        if (targetDistance < 0.1f && targetObject != null)
        {
            duck.Eat(targetObject);
            duck.stateMachine.ChangeState(DuckStateID.Eat);
        }
    }

    private void BeginReach()
    {
        isReaching = true;
        animationDriver = AnimationDriver.IK;
    }

    private void EndReach()
    {
        isReaching = false;
        animationDriver = AnimationDriver.Twist;
    }

    private void Swim(float speedMultiplier)
    {
        thrustTimer -= Time.deltaTime;

        if (thrustTimer <= 0f)
        {
            thrustTimer = duck.globalVars.thrustInterval;
            duck.Swim(duck.speed * duck.globalVars.pursuitSpeedMultiplier * speedMultiplier, false);
        }
    }

    private void UpdateTarget()
    {
        targetObject = duck.nearestFood;
        targetPosition = targetObject.transform.position;

        Vector3 directionToTarget = (targetObject.transform.position - duck.transform.position).normalized;
        Vector3 moveTarget = targetPosition - (directionToTarget * 0.1f);

        duck.SetTarget(moveTarget);
    }

    public override void Exit()
    {
        duck.billOpenness = 0f;
        duck.dipAmount = 0f;
    }

    public override void UpdateNearestFood(GameObject food)
    {
        if (food == null) duck.stateMachine.ChangeState(DuckStateID.Idle);
        else if (food != targetObject) UpdateTarget();
    }
}
