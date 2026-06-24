using System.Text.Json.Serialization;
using GhostPet.Models;

namespace GhostPet.Behaviors;

[BehaviorName("PetBehavior")]
public sealed class PetBehavior : BehaviorBase
{
    [JsonPropertyName("PetStrokes")]
    public int PetStrokes { get; init; } = 5;

    private int _strokeCount;

    public PetBehavior()
    {
        EntersState = GhostState.Petted;
    }

    public override void Execute()
    {
        if (Context?.State == GhostState.Petted) return;

        _strokeCount++;
        if (_strokeCount >= PetStrokes)
        {
            _strokeCount = 0;
            Context?.Say(this);
        }
    }
}
