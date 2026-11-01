using Microsoft.Extensions.DependencyInjection;
using PakuPaku.Common;
using PakuPaku.Common.Audio;
using PakuPaku.Common.Graphics;
using PakuPaku.Common.Input;
using PakuPaku.Common.Window;
using PakuPaku.Domain;
using PakuPaku.Features.Audio;
using PakuPaku.Features.CollisionAndEating;
using PakuPaku.Features.GhostAI;
using PakuPaku.Features.LevelProgression;
using PakuPaku.Features.Movement;
using PakuPaku.Features.Rendering;

namespace PakuPaku;

internal class Program
{
    private static void Main(string[] args)
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<GameState>() // Global state
            .AddSingleton<IGameWindow, RaylibWindow>()
            .AddSingleton<IRenderingContext, RaylibRenderingContext>()
            .AddSingleton<IInputSystem, RaylibInputSystem>()
            .AddSingleton<IAudioService, RaylibAudioService>()
            .AddSingleton<IGameEngine, GameEngine>()
            .AddSingleton<MovementEngine>()
            .AddGhostAISlice()
            .AddMovementSlice()
            .AddRenderingSlice()
            .AddAudioSlice()
            .AddCollisionAndEatingSlice()
            .AddLevelProgressionSlice()
            .BuildServiceProvider();

        try
        {
            var gameEngine = serviceProvider.GetRequiredService<IGameEngine>();
            gameEngine.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
