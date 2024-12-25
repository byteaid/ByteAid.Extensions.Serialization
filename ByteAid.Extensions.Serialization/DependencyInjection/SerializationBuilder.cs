using Microsoft.Extensions.DependencyInjection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace ByteAid.Extensions.DependencyInjection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public class SerializationBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;
}
