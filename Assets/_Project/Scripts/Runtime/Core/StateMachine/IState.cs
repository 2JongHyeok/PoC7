// Purpose: Defines a minimal state contract (SRP: state interface)
public interface IState
{
    void Enter();
    void Tick(float deltaTime);
    void Exit();
}
