using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Linq;

namespace GhostPet.Behaviors;

public static class BehaviorRegistry
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Dictionary<string, Type> _types;

    static BehaviorRegistry()
    {
        _types = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.GetCustomAttribute<BehaviorNameAttribute>() is not null)
            .ToDictionary(t => t.GetCustomAttribute<BehaviorNameAttribute>()!.Name);
    }

    public static BehaviorBase? Create(string name, IBehaviorContext context)
    {
        if (!_types.TryGetValue(name, out var type)) return null;

        try
        {
            var uri = new Uri($"pack://application:,,,/Resources/behaviors/{name}.json");
            var info = Application.GetResourceStream(uri);
            if (info is null) return null;

            var behavior = (BehaviorBase?)JsonSerializer.Deserialize(info.Stream, type, _jsonOptions);
            if (behavior is null) return null;

            behavior.Context = context;

            if (behavior is BehaviorLongTalkBase lt) lt.Setup();

            return behavior;
        }
        catch
        {
            return null;
        }
    }

    public static List<BehaviorBase> CreateAll(IBehaviorContext context) =>
        _types.Keys
              .Select(name => Create(name, context))
              .OfType<BehaviorBase>()
              .ToList();
}
