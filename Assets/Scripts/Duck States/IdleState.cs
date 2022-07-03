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
        yield return new WaitForSeconds(Random.Range(duck.globalVars.minIdleTime, duck.globalVars.maxIdleTime));
        if (duck.state == this)
        {
            if (Random.value < 0.9f) duck.SetState(new WanderState(duck));
            else duck.SetState(new PreenState(duck));
        }
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
