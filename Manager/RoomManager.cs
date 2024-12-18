using Screeps.Manager.Source;
using Screeps.Manager.Spawn;
using ScreepsDotNet.API.World;

namespace Screeps.Manager;

public class RoomManager
{
    private readonly IRoom _room;
    private readonly IGame _game;
    private readonly SpawnManager _spawnManager;
    private readonly BuildManager _buildManager;
    private readonly SourceManager _sourceManager;
    private readonly TowerManager _towerManager;
    
    
    public RoomManager(IGame game, IRoom room)
    {
        _game = game;
        _room = room;
        _sourceManager = new SourceManager(room);
        _spawnManager = new SpawnManager(game, room, _sourceManager);
        _buildManager = new BuildManager(game, room, _sourceManager);
        _towerManager = new TowerManager(room);
    }

    public void Tick()
    {
        _spawnManager.Tick();
        _buildManager.Tick();
        _towerManager.Tick();

        // if (_room.Find<ICreep>().Any(creep => !creep.My) && _room.Controller!.SafeModeAvailable > 0 && _room.Controller!.SafeMode == null)
        // {
        //     _room.Controller.ActivateSafeMode();
        // }
    }
}