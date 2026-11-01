using Microsoft.Extensions.DependencyInjection;

namespace PakuPaku.Features.LevelProgression;

public static class LevelProgressionSliceExtensions
{
    public static IServiceCollection AddLevelProgressionSlice(this IServiceCollection services)
    {
        services.AddTransient<ICheckLevelCompleteCommand, CheckLevelCompleteCommand>();
        return services;
    }
}