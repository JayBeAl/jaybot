using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;
using Screeps.Manager;
using ScreepsDotNet.API.Bot;
using ScreepsDotNet.API.World;

namespace Screeps;

public class JayBot : IBot
{
    private readonly IGame _game;
    private readonly SpawnManager _spawnManager;
    private readonly Dictionary<IRoom, RoomManager> _roomManagers = [];
    
    public JayBot(IGame game)
    {
        _game = game;
        _spawnManager = new SpawnManager(_game);

        CleanMemory();
    }
    
    public void Loop()
    {
        Console.WriteLine("Hello, im existing");
        // Check for any rooms that are no longer visible and remove their manager
        var trackedRooms = _roomManagers.Keys.ToArray();
        foreach (var room in trackedRooms)
        {
            if (room.Exists) { continue; }
            Console.WriteLine($"Removing room manager for {room} as it is no longer visible");
            _roomManagers.Remove(room);
        }

        // Iterate over all visible rooms, create their manager if needed, and tick them
        foreach (var room in _game.Rooms.Values)
        {
            if (!room.Controller?.My ?? false) { continue; }
            if (!_roomManagers.TryGetValue(room, out var roomManager))
            {
                Console.WriteLine($"Adding room manager for {room} as it is now visible and controlled by us");
                roomManager = new RoomManager(_game, room, _spawnManager);
                _roomManagers.Add(room, roomManager);
            }
            roomManager.Tick();
        }
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