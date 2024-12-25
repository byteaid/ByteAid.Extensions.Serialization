using System.Diagnostics.CodeAnalysis;

namespace ByteAid.Extensions.Serialization;

public class PropertySerializationRule : SerializationRule
{
#if NET8_0_OR_GREATER
    public required string Property { get; set; }
    public required int Position { get; set; }
#else
    public string Property { get; set; } = default!;
    public int Position { get; set; }
#endif
    public bool IsRequired { get; set; }
    public string? HeaderName { get; set; }

#if NET8_0_OR_GREATER
    [SetsRequiredMembers]
#endif
    public PropertySerializationRule(string property, int position, bool isRequired, string? headerName = null)
    {
        Property = property;
        Position = position;
        IsRequired = isRequired;
        HeaderName = headerName;
    }

    public PropertySerializationRule()
    {

    }
}
