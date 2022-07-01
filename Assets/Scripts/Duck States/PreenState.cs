using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreenState : DuckState
{
    public PreenState(Duck duck) : base(duck)
    {

    }

    public override IEnumerator Enter()
    {
        duck.animator.SetBool("Preening", true);
        yield return new WaitForSeconds(Random.Range(duck.globalVars.minIdleTime, duck.globalVars.maxIdleTime));
        if(duck.state == this) duck.SetState(new WanderState(duck));
    }


    public override IEnumerator Exit()
    {
        duck.animator.SetBool("Preening", false);
        return base.Exit();
    }

    public override void UpdateNearestFood(GameObject nearest)
    {
        if (nearest != null && nearest.activeInHierarchy) duck.SetState(new PursuitState(duck, nearest));
    }
}
