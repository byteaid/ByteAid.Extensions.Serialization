namespace ByteAid.Extensions.Serialization;

public class SeparatorSerializationRule : SerializationRule
{
#if NET8_0_OR_GREATER
    public required ValueSeparator SeparatorType { get; set; }
#else
    public ValueSeparator SeparatorType { get; set; }
#endif

    public string GetSeparator() => SeparatorType switch
    {
        ValueSeparator.Comma => ",",
        ValueSeparator.Pipe => "|",
        ValueSeparator.Tab => "\t",
        _ => throw new InvalidOperationException($"Unsupported separator type: {SeparatorType}")
    };

#if !NET8_0_OR_GREATER
    public SeparatorSerializationRule()
    {
    }

    public SeparatorSerializationRule(ValueSeparator separatorType)
    {
        SeparatorType = separatorType;
    }
#endif
}