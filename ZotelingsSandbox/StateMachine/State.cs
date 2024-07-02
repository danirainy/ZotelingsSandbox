namespace ZotelingsSandbox.StateMachine;
internal class State
{
    public virtual void Enter(StateMachine stateMachine)
    {
    }
    public virtual string Update(StateMachine stateMachine)
    {
        return null;
    }
    public virtual void Exit(StateMachine stateMachine, bool interrupted)
    {
    }
}
