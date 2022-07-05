using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DuckStateMachine
{
    public DuckState[] states;
    [HideInInspector] public Duck duck;
    public DuckStateID currentState;

    public DuckStateMachine(Duck duck)
    {
        this.duck = duck;
        int numStates = System.Enum.GetNames(typeof(DuckStateID)).Length;
        states = new DuckState[numStates];

        RegisterState(new IdleState(duck));
        RegisterState(new WanderState(duck));
        RegisterState(new EatState(duck));
        RegisterState(new PursuitState(duck));
        RegisterState(new PreenState(duck));
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
        GetState(currentState)?.Update();
    }

    public void ChangeState(DuckStateID newState)
    {
        GetState(currentState)?.Exit();
        currentState = newState;
        GetState(currentState)?.Enter();
    }
}
