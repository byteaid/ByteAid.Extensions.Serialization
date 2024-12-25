using ByteAid.Extensions.Serialization;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace ByteAid.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public class SeparatedTextRuleBuilder<T>
{
    private List<SerializationRule> Rules { get; } = [];

    internal List<SerializationRule> Build() => Rules;

    internal void AddRule(SerializationRule rule) => Rules.Add(rule);
}
