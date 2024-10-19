using System;
using Microsoft.VisualBasic;
using ScreepsDotNet.API.Bot;
using ScreepsDotNet.API.World;

namespace Screeps;

public class JayBot : IBot
{
    private readonly IGame _game;
    
    
    public JayBot(IGame game)
    {
        _game = game;

        CleanMemory();
    }
    
    public void Loop()
    {
        Console.WriteLine("Hello, im existing");
    }
    
    
    private void CleanMemory()
    {
        if (!_game.Memory.TryGetObject("creeps", out var creepsObj)) { return; }

        // Delete all creeps in memory that no longer exist
        var clearCnt = 0;
        foreach (var creepName in creepsObj.Keys)
        {
            if (!_game.Creeps.ContainsKey(creepName))
            {
                creepsObj.ClearValue(creepName);
                ++clearCnt;
            }
        }

        if (clearCnt > 0)
        {
            Console.WriteLine($"Cleared {clearCnt} dead creeps from memory");
        }
    }
}