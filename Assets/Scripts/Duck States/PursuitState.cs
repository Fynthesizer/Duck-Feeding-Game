using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PursuitState : DuckState
{
    private readonly GameObject target;
    private Vector3 targetPosition;

    private float headIKWeight;

    public PursuitState(Duck duck, GameObject _target) : base(duck)
    {
        target = _target;
    }

    public override IEnumerator Enter()
    {
        if (target != null && target.activeInHierarchy) targetPosition = target.transform.position;
        else duck.SetState(new IdleState(duck));

        headIKWeight = 0.0f;
        return base.Enter();
    }

    public override void Swim()
    {
        duck.Swim(targetPosition, duck.speed * duck.globalVars.pursuitSpeedMultiplier, duck.globalVars.pursuitAvoidLayers);
    }

    public override void Update()
    {
        duck.headIK.weight = Mathf.Lerp(duck.headIK.weight, headIKWeight, Time.deltaTime * 5f);
        duck.lookConstraint.weight = Mathf.Lerp(duck.lookConstraint.weight, 1 - headIKWeight, Time.deltaTime * 5f);

        if (target != null && target.activeInHierarchy) { 
            float targetDistance = Vector3.Distance(duck.transform.position, targetPosition);
            float targetDot = Vector3.Dot(duck.transform.forward, (targetPosition - duck.transform.position).normalized);
            Debug.Log(targetDot);
            if (targetDistance < 0.3f)
            {
                duck.Eat(target);
            }

            if (targetDistance < 1f && targetDot > 0.6f)
            {
                duck.headIK.transform.GetChild(0).position = target.transform.position;
                headIKWeight = (1f - targetDistance);
            }
        }
    }

    public override IEnumerator Exit()
    {
        headIKWeight = 0f;
        duck.lookConstraint.weight = 1f;
        while(duck.headIK.weight > 0f)
        {
            duck.headIK.weight -= 0.04f;
            yield return null;
        }
    }

    public override void UpdateNearestFood(GameObject nearest)
    {
        if (nearest != target) duck.SetState(new PursuitState(duck, nearest));
        else if (nearest == null) duck.SetState(new IdleState(duck));
    }
}
