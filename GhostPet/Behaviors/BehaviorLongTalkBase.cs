using System.Text.Json.Serialization;
using GhostPet.Models;

namespace GhostPet.Behaviors;

public abstract class BehaviorLongTalkBase : BehaviorBase
{
    [JsonPropertyName("stmtChains")]
    public string[][]? StmtChains { get; init; }

    [JsonPropertyName("timeChains")]
    public int[][]? TimeChains { get; init; }

    // Null entries mean "use previous image in chain; null at index 0 means use default"
    [JsonPropertyName("imgChains")]
    public string?[][]? ImgChains { get; init; }

    [JsonIgnore]
    public int ChainIndex { get; set; }

    [JsonIgnore]
    public int ChainUsed { get; private set; }

    protected BehaviorLongTalkBase()
    {
        EntersState = GhostState.Speaking;
    }

    public void Setup()
    {
        if (StmtChains is { Length: > 0 })
            ChainUsed = Random.Shared.Next(StmtChains.Length);
    }

    public void PickNextChain()
    {
        ChainIndex = 0;
        if (StmtChains is { Length: > 1 })
            ChainUsed = Random.Shared.Next(StmtChains.Length);
    }

    public string ResolveImage(int chain, int idx)
    {
        if (ImgChains is null) return "speak.png";
        for (int i = idx; i >= 0; i--)
        {
            if (ImgChains[chain][i] is { } img) return img;
        }
        return "stand.png";
    }
}
