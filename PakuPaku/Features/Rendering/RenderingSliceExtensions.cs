using Microsoft.Extensions.DependencyInjection;

namespace PakuPaku.Features.Rendering;

public static class RenderingSliceExtensions
{
    public static IServiceCollection AddRenderingSlice(this IServiceCollection services)
    {
        services.AddTransient<IDrawFrameCommand, DrawFrameCommand>();
        return services;
    }
}