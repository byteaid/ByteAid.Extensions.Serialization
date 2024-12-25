using ByteAid.Extensions.Serialization;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace ByteAid.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class SerializationExtensions
{
    public static SerializationBuilder AddSerialization(this IServiceCollection services, Action<SerializationBuilder> configure)
    {
        services.AddSingleton<ISerializationRuleset, SerializationRuleset>();
        services.AddSingleton<ISerializationHelper, SerializationHelper>();

        var builder = new SerializationBuilder(services);
        configure(builder);

        return builder;
    }
}
