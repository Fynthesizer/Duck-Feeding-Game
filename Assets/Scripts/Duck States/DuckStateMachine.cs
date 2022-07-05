using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckStateMachine
{
    public DuckState[] states;
    public Duck duck;
    public DuckStateID currentState;

    public DuckStateMachine(Duck duck)
    {
        this.duck = duck;
        int numStates = System.Enum.GetNames(typeof(DuckStateID)).Length;
        states = new DuckState[numStates];
    }

    public void RegisterState(DuckState state)
    {
        int index = (int)state.GetID();
        states[index] = state;
    }

    public DuckState GetState(DuckStateID stateID)
    {
        int index = (int)stateID;
        return states[index];
    }

    public void Update()
    {
        GetState(currentState)?.Update(duck);
    }

    public void ChangeState(DuckStateID newState)
    {
        GetState(currentState)?.Exit(duck);
        currentState = newState;
        GetState(currentState)?.Enter(duck);
    }
}
