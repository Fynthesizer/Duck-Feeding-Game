using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : DuckState
{
    public IdleState(Duck duck) : base(duck)
    {

    }

    public override IEnumerator Enter()
    {
        yield return new WaitForSeconds(Random.Range(duck.MinIdleTime, duck.MaxIdleTime));
        if(duck.state == this) duck.SetState(new WanderState(duck));
    }


    public override IEnumerator Exit()
    {
        return base.Exit();
    }

    public override void UpdateNearestFood(GameObject nearest)
    {
        if (nearest != null && nearest.activeInHierarchy) duck.SetState(new PursuitState(duck, nearest));
    }
}
