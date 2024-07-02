namespace ZotelingsSandbox.StateMachine;
internal class StateMachine : MonoBehaviour
{
    private void EnterCurrentState()
    {
        var state = states[currentState];
        Log.LogKey("StateMachine", $"{GetType().Name}: Entering state {state.GetType().Name}");
        state.Enter(this);
    }
    private void ExitCurrentState(bool interrupted)
    {
        var state = states[currentState];
        Log.LogKey("StateMachine", $"{GetType().Name}: Exiting state {state.GetType().Name}");
        state.Exit(this, interrupted);
    }
    private void Update()
    {
        if (currentState == null)
        {
            currentState = StartState;
            EnterCurrentState();
        }
        var nextState = states[currentState].Update(this);
        if (nextState != null)
        {
            if (!states.ContainsKey(nextState))
            {
                Log.LogError($"{GetType().Name}: Invalid state {nextState} to transition to");
            }
            ExitCurrentState(false);
            currentState = nextState;
            EnterCurrentState();
        }
    }
    public virtual bool CanTakeDamage()
    {
        Log.LogError($"{GetType().Name}: CanTakeDamage not implemented");
        return true;
    }
    public virtual void TakeDamage(GameObject source, CollisionSide damageSide, int damageAmount, int hazardType)
    {
        Log.LogError($"{GetType().Name}: TakeDamage not implemented");
    }
    protected void AddState(State state)
    {
        states.Add(state.GetType().Name, state);
    }
    public void SetState(string state)
    {
        if (!states.ContainsKey(state))
        {
            Log.LogError($"{GetType().Name}: Invalid state {state} to set to");
        }
        ExitCurrentState(true);
        currentState = state;
        EnterCurrentState();
    }
    private Dictionary<string, State> states = [];
    protected string StartState { get; set; }
    private string currentState;
}
