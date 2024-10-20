using System;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace Screeps.Manager;

public class SpawnManager
{
    private readonly IGame _game;
    private readonly Random _random = new();
    
    private readonly BodyType<BodyPartType> _workerBodyType = new([(BodyPartType.Move, 1), (BodyPartType.Carry, 1), (BodyPartType.Work, 1)]);

    public SpawnManager(IGame game)
    {
        _game = game;
    }
    
    public void TrySpawnCreep(IStructureSpawn spawn, string roleName)
    {
        var name = FindUniqueCreepName(roleName);
        if (spawn.SpawnCreep(_workerBodyType, name, new(dryRun: true)) == SpawnCreepResult.Ok)
        {
            Console.WriteLine($"{this}: spawning a {roleName} ({_workerBodyType}) from {spawn}...");
            var initialMemory = _game.CreateMemoryObject();
            initialMemory.SetValue("role", roleName);
            spawn.SpawnCreep(_workerBodyType, name, new(dryRun: false, memory: initialMemory));
        }
    }
    
    private string FindUniqueCreepName(string prefix)
        => $"{prefix}_{_random.Next()}";
}