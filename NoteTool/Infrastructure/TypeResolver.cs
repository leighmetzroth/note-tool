namespace NoteTool.Infrastructure;

using Spectre.Console.Cli;

public class TypeResolver : ITypeResolver, IDisposable
{
    private readonly IServiceProvider services;

    public TypeResolver(IServiceProvider provider)
    {
        services = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public object? Resolve(Type? type) => type is null ? null : services.GetService(type);

    public void Dispose()
    {
        if (services is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}