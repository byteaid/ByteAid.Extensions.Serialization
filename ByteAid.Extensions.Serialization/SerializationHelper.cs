namespace ByteAid.Extensions.Serialization;

public class SerializationHelper(IEnumerable<ISerializationRuleset> rulesets) : ISerializationHelper
{
    public List<ISerializationRuleset> Rulesets { get; } = new(rulesets);
}
