// File: Helpers/ServiceHelper.cs
using Microsoft.Extensions.DependencyInjection;

namespace ClassScheduleApp.Helpers;

public static class ServiceHelper
{
    private static IServiceProvider? _provider;

    public static void Initialize(IServiceProvider provider) => _provider = provider;

    public static T GetRequiredService<T>() where T : notnull
        => _provider!.GetRequiredService<T>();
}
