using Microsoft.Extensions.DependencyInjection;

namespace PakuPaku.Features.CollisionAndEating;

public static class CollisionAndEatingSliceExtensions
{
    public static IServiceCollection AddCollisionAndEatingSlice(this IServiceCollection services)
    {
        services.AddTransient<IProcessCollisionsCommand, ProcessCollisionsCommand>();
        return services;
    }
}