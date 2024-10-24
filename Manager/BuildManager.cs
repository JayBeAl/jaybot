using System;
using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace Screeps.Manager;

public class BuildManager
{
    private const int BuildIntervalInSeconds = 60;
    private int _buildTickCounter = 0;
    
    private readonly IRoom _room;
    private List<IStructureSpawn> _spawns = [];
    private readonly List<ISource> _sources;
    private readonly List<Position> _roads = [];
    
    public BuildManager(IRoom room)
    {
        _room = room;
        _sources = _room.Find<ISource>().ToList();
        UpdateSpawns();
        GenerateRoomRoutes();
    }

    public void Tick()
    {
        _buildTickCounter++;
        
        if (_buildTickCounter < BuildIntervalInSeconds)
        {
            return;
        }
        
        _buildTickCounter = 0;
        Console.WriteLine($"Checking for roads in {_room.Name}");
        ManageRoads();
    }

    private void UpdateSpawns()
    {
        var existingSpawns = _room.Find<IStructureSpawn>().Where(spawn => spawn.Exists).ToList();
        if (existingSpawns.Count > _spawns.Count(spawn => spawn.Exists))
        {
            _spawns = existingSpawns;
        }
    }

    private void GenerateRoomRoutes()
    {
        foreach (var spawn in _spawns)
        {
            foreach (var source in _sources)
            {
                var sourcePath = _room.FindPath(spawn.RoomPosition, source.RoomPosition, new FindPathOptions(true));
                foreach (var pathStep in sourcePath)
                {
                    _roads.Add(pathStep.Position);
                }
            }
            
            var controllerPath = _room.FindPath(spawn.RoomPosition, _room.Controller!.RoomPosition, new FindPathOptions(true));
            foreach (var pathStep in controllerPath)
            {
                _roads.Add(pathStep.Position);
            }
        }
    }

    private void ManageRoads()
    {
        // Early exit if any construction is in progress
        if (_room.Find<IConstructionSite>().Any())
        {
            var constructionSites = _room.Find<IConstructionSite>();
            foreach (var constructionSite in constructionSites)
            {
                Console.WriteLine("Type:" + constructionSite.StructureType);
                Console.WriteLine("Pos:" + constructionSite.LocalPosition);
            }
            Console.WriteLine("Buildings are being built, skipping road management -> " + _room.Find<IConstructionSite>().First().StructureType);
            return;
        }

        foreach (var roadPosition in _roads)
        {
            var lookResult = _room.LookAt(roadPosition).ToList();
        
            // Skip if road or construction site exists
            if (lookResult.Any(obj => obj is IStructureRoad or IConstructionSite))
            {
                Console.WriteLine($"Found {lookResult.Count} objects at {roadPosition.ToString()}");
                continue;
            }
        
            // Create road construction site if only a creep is present
            if (lookResult.Count == 1 && lookResult[0] is ICreep)
            {
                _room.CreateConstructionSite<IStructureRoad>(roadPosition);
                Console.WriteLine($"Building road at {roadPosition}");
                return;
            }

            foreach (var roomObject in lookResult)
            {
                Console.WriteLine($"Found {roomObject.GetType()}");
            }
        }
    }
}