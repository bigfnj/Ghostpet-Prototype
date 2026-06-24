namespace GhostPet.Behaviors;

[BehaviorName("AskTestBehavior")]
public sealed class AskTestBehavior : BehaviorAskBase
{
    public override void Execute()
    {
        Context?.Say(this);
    }
}
