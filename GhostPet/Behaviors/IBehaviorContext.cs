using GhostPet.Models;

namespace GhostPet.Behaviors;

public interface IBehaviorContext
{
    string GhostName { get; }
    string UserName { get; }
    GhostState State { get; set; }
    void Say(BehaviorBase behavior);
    BehaviorBase? GetBehavior(string name);
    void Reset();
}
