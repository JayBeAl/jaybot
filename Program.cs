using System;
using System.Diagnostics.CodeAnalysis;
using Screeps;
using ScreepsDotNet.API.Bot;
using ScreepsDotNet.API.World;

namespace ScreepsDotNet
{
    public static partial class Program
    {
        private static IGame? _game;
        private static IBot? _bot;

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(Program))]
        public static void Main()
        {
            // Keep the entrypoint platform independent and let Init (which is called from js) create the game instance
            // This keeps the door open for unit testing later down the line
        }

        [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
        public static void Init()
        {
            try
            {
                _game = new Native.World.NativeGame();
                _bot = new JayBot(_game);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
        public static void Loop()
        {
            if (_game == null) { return; }
            try
            {
                _game.Tick();
                _bot?.Loop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}