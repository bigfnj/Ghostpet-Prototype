using System.Text.Json.Serialization;
using GhostPet.Models;

namespace GhostPet.Behaviors;

public abstract class BehaviorBase
{
    [JsonPropertyName("imageUsed")]
    public string? ImageUsed { get; init; }

    [JsonPropertyName("statements")]
    public string[]? Statements { get; init; }

    [JsonPropertyName("actTime")]
    public int ActTime { get; init; } = 3;

    [JsonIgnore]
    public GhostState EntersState { get; protected set; } = GhostState.Speaking;

    [JsonIgnore]
    public IBehaviorContext? Context { get; set; }

    public string GetRandomStatement() =>
        Statements is { Length: > 0 }
            ? Statements[Random.Shared.Next(Statements.Length)]
            : string.Empty;

    public abstract void Execute();
}
