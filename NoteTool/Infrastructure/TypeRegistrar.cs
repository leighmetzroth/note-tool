using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace NoteTool.Infrastructure;

public class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection services;

    public TypeRegistrar(IServiceCollection services)
    {
        this.services = services;
    }

    public void Register(Type service, Type implementation)
    {
        services.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        services.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        services.AddSingleton(service, _ => factory());
    }

    public ITypeResolver Build() => new TypeResolver(services.BuildServiceProvider());
}