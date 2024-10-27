using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Screeps.Roles;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace Screeps.Manager.Spawn;

public class SpawnManager
{
    private readonly IGame _game;
    private readonly IRoom _room;
    private readonly Random _random = new();
    
    private readonly List<ICreep> _allCreeps = [];
    private readonly Dictionary<Role, IRole> _roleMap = [];
    private readonly Dictionary<IRole, Role> _reversedRoleMap;
    
    private readonly List<IStructureSpawn> _roomSpawns;
    private readonly Dictionary<Role, int> _creepCounter = new();
    private readonly Dictionary<Role, int> _creepTargets = new();
    
    private readonly BodyType<BodyPartType> _workerBodyType = new([(BodyPartType.Move, 1), (BodyPartType.Carry, 1), (BodyPartType.Work, 1)]);

    public SpawnManager(IGame game, IRoom room)
    {
        _game = game;
        _room = room;

        _roleMap.Add(Role.Harvester, new Harvester(_room));
        _roleMap.Add(Role.Upgrader, new Upgrader(_room));
        _roleMap.Add(Role.Builder, new Builder(_room));
        _roleMap.Add(Role.Maintainer, new Maintainer(_room));
        _reversedRoleMap = _roleMap.ToDictionary(x => x.Value, x => x.Key);
        
        _creepCounter.Add(Role.Harvester, 0);
        _creepCounter.Add(Role.Upgrader, 0);
        _creepCounter.Add(Role.Builder, 0);
        _creepCounter.Add(Role.Maintainer, 0);
        
        _creepTargets.Add(Role.Harvester, 5);
        _creepTargets.Add(Role.Upgrader, 3);
        _creepTargets.Add(Role.Builder, 2);
        _creepTargets.Add(Role.Maintainer, 1);
        
        // Get all spawns in this room since Room.finds are expensive
        _roomSpawns = _room.Find<IStructureSpawn>().ToList();
    }

    public void Tick()
    {
        // Check for dead creeps and remove them
        foreach (var creep in _allCreeps.Where(creep => !creep.Exists).ToImmutableArray())
        {
            Console.WriteLine($"Removing dead creep {creep}");
            OnDead(creep);
        }
        
        // Find new creeps
        var newCreepsList = _room.Find<ICreep>().Where(creep => !_allCreeps.Contains(creep));
        foreach (var creep in newCreepsList)
        {
            OnSpawn(creep);
        }
        
        // Spawn creeps when needed
        foreach (var spawn in _roomSpawns.Where(spawn => spawn.Exists))
        {
            TickSpawn(spawn);
        }
        
        // Tick all creeps
        foreach (var creep in _allCreeps)
        {
            TickCreep(creep);
        }
    }
    
    private void TickSpawn(IStructureSpawn spawn)
    {
        if (spawn.Spawning != null)
        {
            return;
        }

        foreach (var role in Enum.GetValues<Role>())
        {
            if (_creepCounter[role] < _creepTargets[role])
            {
                TrySpawnCreep(spawn, role);
                break;
            }
        }
    }

    private void OnSpawn(ICreep creep)
    {
        var roleInstance = GetCreepRole(creep);
        if (roleInstance == null)
        {
            return;
        }

        if (_reversedRoleMap.TryGetValue(roleInstance, out var value) && !_allCreeps.Contains(creep))
        {
            _allCreeps.Add(creep);
            _creepCounter[value]++;
            Console.WriteLine($"Spawned creep with role {value.ToString()}");
        }
        else
        {
            Console.WriteLine($"Creep with unknown role spawned -> {roleInstance}");
        }
    }

    private void OnDead(ICreep creep)
    {
        var roleInstance = GetCreepRole(creep);
        if (roleInstance == null)
        {
            return;
        }

        if (_reversedRoleMap.TryGetValue(roleInstance, out var value) && _allCreeps.Contains(creep))
        {
            _allCreeps.Remove(creep);
            _creepCounter[value]--;
            Console.WriteLine($"Removed creep with role {value.ToString()}");
        }
        else
        {
            Console.WriteLine($"Creep with unknown role died -> {roleInstance}");
        }
    }

    private void TickCreep(ICreep creep)
    {
        var role = GetCreepRole(creep);
        if (role == null)
        {
            return;
        }
        role.Run(creep);
    }
    
    private void TrySpawnCreep(IStructureSpawn spawn, Role role)
    {
        var name = FindUniqueCreepName(role.ToString());
        if (spawn.SpawnCreep(_workerBodyType, name, new(dryRun: true)) == SpawnCreepResult.Ok)
        {
            Console.WriteLine($"{this}: spawning a {role.ToString()} ({_workerBodyType}) from {spawn}...");
            var initialMemory = _game.CreateMemoryObject();
            initialMemory.SetValue("role", (int)role);
            spawn.SpawnCreep(_workerBodyType, name, new(dryRun: false, memory: initialMemory));
        }
    }
    
    private IRole? GetCreepRole(ICreep creep)
    {
        // First, see if we've stored the role instance on the creep from a previous tick (this will save us some CPU)
        if (creep.TryGetUserData<IRole>(out var roleInstance)) { return roleInstance; }

        // Lookup their role from memory
        if (!creep.Memory.TryGetInt("role", out var role)) { return null; }

        // Lookup the role instance
        if (!_roleMap.TryGetValue((Role)role, out roleInstance)) { return null; }

        // We found it, assign it to the creep user data for later retrieval
        creep.SetUserData(roleInstance);
        return roleInstance;
    }

    private void OutputExistingCreeps(string callPosition)
    {
        Console.WriteLine($"Existing creeps {callPosition}:");
        foreach (var creep in _allCreeps)
        {
            Console.WriteLine($"{creep.Name}");
        }
    }
    
    private string FindUniqueCreepName(string prefix)
        => $"{prefix}_{_random.Next()}";
}