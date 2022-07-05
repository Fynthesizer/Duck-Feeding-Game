using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreenState : DuckState
{

    public override bool allowQuack { get { return false; } }
    public override bool allowLook { get { return false; } }
    public PreenState(Duck duck) : base(duck)
    {

    }

    public override IEnumerator Enter()
    {
        duck.animator.SetBool("Mirror", Random.value > 0.5f);
        duck.animator.SetBool("Preening", true);
        yield return new WaitForSeconds(Random.Range(duck.globalVars.minIdleTime, duck.globalVars.maxIdleTime));
        if (duck.state == this)
        {
            if (Random.value > 0.5f) duck.SetState(new WanderState(duck));
            else duck.SetState(new IdleState(duck));
        }
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
