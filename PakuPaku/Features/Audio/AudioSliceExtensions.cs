using Microsoft.Extensions.DependencyInjection;

namespace PakuPaku.Features.Audio;

public static class AudioSliceExtensions
{
    public static IServiceCollection AddAudioSlice(this IServiceCollection services)
    {
        services.AddTransient<IAmbientAudioSystem, AmbientAudioSystem>();
        return services;
    }
}