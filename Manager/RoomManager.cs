using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Screeps.Roles;
using ScreepsDotNet.API.World;

namespace Screeps.Manager;

public class RoomManager
{
    private readonly IRoom _room;
    private readonly IGame _game;
    private readonly SpawnManager _spawnManager;
    private readonly BuildManager _buildManager;

    private readonly Dictionary<string, IRole> _roleMap = [];

    private const int HarvesterTarget= 5;
    private const int UpgraderTarget= 3;
    private const int BuilderTarget= 2;
    private const int MaintainerTarget= 1;
    
    private List<ICreep> _allCreeps = [];
    private List<ICreep> _harvesters = [];
    private List<ICreep> _upgraders = [];
    private List<ICreep> _builders = [];
    private List<ICreep> _maintainers = [];
    
    private List<IStructureSpawn> _roomSpawns = [];
    
    
    public RoomManager(IGame game, IRoom room, SpawnManager spawnManager)
    {
        _game = game;
        _room = room;
        _spawnManager = spawnManager;
        _buildManager = new BuildManager(game, room);

        _roleMap.Add("harvester", new Harvester(_room));
        _roleMap.Add("upgrader", new Upgrader(_room));
        _roleMap.Add("builder", new Builder(_room));
        _roleMap.Add("maintainer", new Maintainer(_room));
        
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
        
        _buildManager.Tick();
    }

    private void TickSpawn(IStructureSpawn spawn)
    {
        if (spawn.Spawning != null)
        {
            return;
        }
        
        if (_harvesters.Count < HarvesterTarget)
        {
            _spawnManager.TrySpawnCreep(spawn, "harvester");
        }
        else if (_upgraders.Count < UpgraderTarget)
        {
            _spawnManager.TrySpawnCreep(spawn, "upgrader");
        }
        else if (_builders.Count < BuilderTarget)
        {
            _spawnManager.TrySpawnCreep(spawn, "builder");
        }
        else if (_maintainers.Count < MaintainerTarget)
        {
            _spawnManager.TrySpawnCreep(spawn, "maintainer");
        }
    }

    private void OnSpawn(ICreep creep)
    {
        _allCreeps.Add(creep);
        var role = GetCreepRole(creep);
        {
            switch (role)
            {
                case Harvester:
                    _harvesters.Add(creep);
                    Console.WriteLine($"Added {role}");
                    break;
                case Upgrader:
                    _upgraders.Add(creep);
                    Console.WriteLine($"Added {role}");
                    break;
                case Builder:
                    _builders.Add(creep);
                    Console.WriteLine($"Added {role}");
                    break;
                case Maintainer:
                    _maintainers.Add(creep);
                    Console.WriteLine($"Added {role}");
                    break;
                default:
                    Console.WriteLine($"Unknown creep role {role}");
                    break;
            }
        }
    }

    private void OnDead(ICreep creep)
    {
        _allCreeps.Remove(creep);
        var role = GetCreepRole(creep);
        {
            switch (role)
            {
                case Harvester:
                    _harvesters.Remove(creep);
                    Console.WriteLine($"Removed {role}");
                    break;
                case Upgrader:
                    _upgraders.Remove(creep);
                    Console.WriteLine($"Removed {role}");
                    break;
                case Builder:
                    _builders.Remove(creep);
                    Console.WriteLine($"Removed {role}");
                    break;
                case Maintainer:
                    _maintainers.Remove(creep);
                    Console.WriteLine($"Removed {role}");
                    break;
                default:
                    Console.WriteLine($"Unknown creep role {role}");
                    break;
            }
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
    
    private IRole? GetCreepRole(ICreep creep)
    {
        // First, see if we've stored the role instance on the creep from a previous tick (this will save us some CPU)
        if (creep.TryGetUserData<IRole>(out var role)) { return role; }

        // Lookup their role from memory
        if (!creep.Memory.TryGetString("role", out var roleName)) { return null; }

        // Lookup the role instance
        if (!_roleMap.TryGetValue(roleName, out role)) { return null; }

        // We found it, assign it to the creep user data for later retrieval
        creep.SetUserData(role);
        return role;
    }

    private void OutputExistingCreeps(string callPosition)
    {
        Console.WriteLine($"Existing creeps {callPosition}:");
        foreach (var creep in _allCreeps)
        {
            Console.WriteLine($"{creep.Name}");
        }
    }
}