namespace ByteAid.Extensions.Serialization;

public interface ISerializationRuleset
{
    Type Target { get; set; }
    List<SerializationRule> Rules { get; set; }
}
