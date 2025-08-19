namespace AI.Command
{
    public abstract class AICommand
    {
        public abstract void Execute(Blackboard.Blackboard blackboard);
    }
}