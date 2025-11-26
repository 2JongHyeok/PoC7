// Purpose: Provides shared logging for concrete states (SRP: base state utilities)
using UnityEngine;

public abstract class BaseState : IState
{
    protected readonly string StateName;

    protected BaseState(string stateName)
    {
        StateName = stateName;
    }

    public virtual void Enter()
    {
        Debug.Log($"[State] Enter {StateName}");
    }

    public virtual void Tick(float deltaTime)
    {
    }

    public virtual void Exit()
    {
        Debug.Log($"[State] Exit {StateName}");
    }
}
