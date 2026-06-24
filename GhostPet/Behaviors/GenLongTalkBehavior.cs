namespace GhostPet.Behaviors;

[BehaviorName("GenLongTalkBehavior")]
public sealed class GenLongTalkBehavior : BehaviorLongTalkBase
{
    public override void Execute()
    {
        Context?.Say(this);
    }
}
