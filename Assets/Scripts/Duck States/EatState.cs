using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatState : DuckState
{
    public override bool allowQuack { get { return false; } }
    public override bool allowLook { get { return false; } }

    public EatState(Duck duck) : base(duck)
    {

    }

    public override IEnumerator Enter()
    {
        yield return new WaitForSeconds(duck.globalVars.eatTime);
        if (duck.state == this) duck.SetState(new WanderState(duck));
    }
    
    public override IEnumerator Exit()
    {
        return base.Exit();
    }
}
