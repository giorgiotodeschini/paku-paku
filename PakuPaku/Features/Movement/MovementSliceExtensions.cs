using Microsoft.Extensions.DependencyInjection;

namespace PakuPaku.Features.Movement;

public static class MovementSliceExtensions
{
    public static IServiceCollection AddMovementSlice(this IServiceCollection services)
    {
        services.AddTransient<IMoveAndEatCommand, MoveAndEatCommand>();
        services.AddTransient<IMoveGhostsCommand, MoveGhostsCommand>();
        return services;
    }
}