// Purpose: Runs and switches IState instances (SRP: state lifecycle runner)
using UnityEngine;

public sealed class StateMachine
{
    private IState _current;

    public string CurrentName => _current?.GetType().Name ?? "None";

    public void SetState(IState next)
    {
        if (_current == next) return;
        _current?.Exit();
        _current = next;
        _current?.Enter();
        Debug.Log($"[StateMachine] Switched to {CurrentName}");
    }

    public void Tick(float deltaTime)
    {
        _current?.Tick(deltaTime);
    }
}
