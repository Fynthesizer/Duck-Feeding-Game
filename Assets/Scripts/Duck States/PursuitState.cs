using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PursuitState : DuckState
{
    private readonly GameObject target;
    private Vector3 targetPosition;

    public PursuitState(Duck duck, GameObject _target) : base(duck)
    {
        target = _target;
    }

    public override IEnumerator Enter()
    {
        if (target != null && target.activeInHierarchy) targetPosition = target.transform.position;
        else duck.SetState(new IdleState(duck));
        return base.Enter();
    }

    public override void Swim()
    {
        duck.Swim(targetPosition, duck.speed * duck.PursuitSpeedMultiplier, duck.PursuitAvoidLayers);
    }

    public override void Update()
    {
        if(target != null && target.activeInHierarchy) { 
            float targetDistance = Vector3.Distance(duck.transform.position, targetPosition);
            if (targetDistance < 0.5f) duck.Eat(target);
        }
    }

    public override void UpdateNearestFood(GameObject nearest)
    {
        if (nearest != target) duck.SetState(new PursuitState(duck, nearest));
        else if (nearest == null) duck.SetState(new IdleState(duck));
    }
}
