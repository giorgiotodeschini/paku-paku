using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace PakuPaku.Features.GhostAI;

public static class GhostAISliceExtensions
{
    public static IServiceCollection AddGhostAISlice(this IServiceCollection services)
    {
        services.AddTransient<IProcessGhostAISystem, ProcessGhostAISystem>();
        return services;
    }
}