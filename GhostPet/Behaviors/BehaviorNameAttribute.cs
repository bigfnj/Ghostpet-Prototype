namespace GhostPet.Behaviors;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class BehaviorNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
