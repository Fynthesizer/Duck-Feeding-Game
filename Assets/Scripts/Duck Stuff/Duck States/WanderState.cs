using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderState : DuckState
{
    private float thrustTimer = 0f;

    public override DuckStateID GetID()
    {
        return DuckStateID.Wander;
    }
    private Vector3 targetPosition;
    private float targetDistance;

    public WanderState(Duck duck) : base(duck)
    {

    }

    public override void Enter()
    {
        thrustTimer = 0.2f;

        Vector3 newTarget;
        int attempts = 0;
        while (true)
        {
            Vector2 targetOffset = Random.insideUnitCircle * duck.globalVars.wanderRadius;
            newTarget = duck.transform.position + new Vector3(targetOffset.x, 0, targetOffset.y);
            Vector3 newTargetDirection = newTarget - duck.transform.position;
            float newTargetDistance = Vector3.Distance(duck.transform.position, newTarget);
            attempts++;
            if (GameManager.Instance.PositionIsOnLake(newTarget) && duck.PathIsClear(newTargetDirection, newTargetDistance, duck.globalVars.wanderAvoidLayers)) break;
            else if (attempts > 50)
            {
                newTarget = duck.transform.position;
                break;
            }
        }
        targetPosition = newTarget;
        //yield break;
    }

    public override void Update()
    {
        Vector3 targetDirection = (targetPosition - duck.transform.position).normalized;
        duck.targetLookDirection = -targetDirection;

        Swim();
        CheckTargetDistance();
    }

    private void Swim()
    {
        thrustTimer -= Time.deltaTime;

        if (thrustTimer <= 0f)
        {
            thrustTimer = 0.2f;
            duck.Swim(targetPosition, duck.speed, duck.globalVars.wanderAvoidLayers);
        }
    }

    private void CheckTargetDistance()
    {
        targetDistance = Vector3.Distance(duck.transform.position, targetPosition);

        if (targetDistance < 0.1f)
        {
            if (Random.value > 0.5f) duck.stateMachine.ChangeState(DuckStateID.Idle);
            else duck.stateMachine.ChangeState(DuckStateID.Wander);
        }
    }

    /*
    public override void Swim()
    {
        duck.Swim(targetPosition, duck.speed, duck.globalVars.wanderAvoidLayers);
    }
    */

    public override void UpdateNearestFood(GameObject food)
    {
        if (food != null) duck.stateMachine.ChangeState(DuckStateID.Pursuit);
    }
}
