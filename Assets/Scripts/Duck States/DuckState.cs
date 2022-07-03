using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DuckState
{
    protected Duck duck;
    public virtual bool allowQuack { get { return true; } }

    public DuckState(Duck _duck)
    {
        duck = _duck;
    }

    public virtual IEnumerator Enter()
    {
        yield break;
    }


    public virtual IEnumerator Exit()
    {
        yield break;
    }

    public virtual void Swim()
    {
    }

    public virtual void Update()
    {

    }


    public virtual void UpdateNearestFood(GameObject nearest)
    {
    }
}


