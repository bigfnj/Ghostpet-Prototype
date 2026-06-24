using System.Text.Json.Serialization;
using GhostPet.Models;

namespace GhostPet.Behaviors;

public abstract class BehaviorAskBase : BehaviorBase
{
    [JsonPropertyName("actStatements")]
    public string[]? ActStatements { get; init; }

    // Names of behaviors to invoke per choice; resolved at runtime via IBehaviorContext
    [JsonPropertyName("BehaviorList")]
    public string[]? BehaviorList { get; init; }

    protected BehaviorAskBase()
    {
        EntersState = GhostState.Asking;
    }
}
