using GhostPet.Models;

namespace GhostPet.Behaviors;

// Used as the cancel/timeout behavior for Ask dialogs
[BehaviorName("GiveUpBehavior")]
public sealed class GiveUpBehavior : BehaviorBase
{
    public GiveUpBehavior()
    {
        EntersState = GhostState.Speaking;
    }

    public override void Execute()
    {
        Context?.Say(this);
    }
}
