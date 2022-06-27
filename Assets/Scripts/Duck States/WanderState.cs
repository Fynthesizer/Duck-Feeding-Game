using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderState : DuckState
{
    private Vector3 targetPosition;

    public WanderState(Duck duck) : base(duck)
    {

    }

    public override IEnumerator Enter()
    {
        //duck.PickWanderTarget();
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
        yield break;
    }

    public override IEnumerator Exit()
    {
        return base.Exit();
    }

    public override void Swim()
    {
        duck.Swim(targetPosition, duck.speed, duck.globalVars.wanderAvoidLayers);
    }

    public override void UpdateNearestFood(GameObject nearest)
    {
        if (nearest != null && nearest.activeInHierarchy) duck.SetState(new PursuitState(duck, nearest));
    }
}
