namespace ByteAid.Extensions.Serialization;

public class SerializationRuleset : ISerializationRuleset
{
#if NET8_0_OR_GREATER
    public required Type Target { get; set; }
#else
    public Type Target { get; set; } = default!;
#endif
    public List<SerializationRule> Rules { get; set; } = [];
}
