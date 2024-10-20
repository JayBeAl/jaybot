using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace Screeps.Roles;

public class Harvester(IRoom room) : RoleBase(room)
{
    private bool _isMining;
    public override void Run(ICreep creep)
    {
        if (_isMining && creep.Store.GetFreeCapacity() == 0)
        {
            _isMining = false;
            creep.Say("Delivering \ud83d\udea7");
        }
        
        if (!_isMining && creep.Store.GetUsedCapacity() == 0)
        {
            _isMining = true;
            creep.Say("Mining \u26a1");
        }
        
        if (creep.Store.GetFreeCapacity() > 0)
        {
            var source = FindNearestSource(creep.LocalPosition);
            if (source == null)
            {
                return;
            }
            
            if (creep.Harvest(source) == CreepHarvestResult.NotInRange)
            {
                creep.MoveTo(source.LocalPosition, new MoveToOptions());
            }
        }
        else
        {
            var storage = FindNearestEnergyStorageWithSpace(creep.LocalPosition);
            if (storage == null)
            {
                creep.Say("No energy storage available.");
                return;
            }
            
            if (creep.Transfer(storage, ResourceType.Energy) == CreepTransferResult.NotInRange)
            {
                creep.MoveTo(storage.RoomPosition);
            }
        }
    }

    private ISource? FindNearestSource(Position position)
    {
        return _room.Find<ISource>().MinBy(x => x.LocalPosition.LinearDistanceTo(position));
    }
    
    private IStructure? FindNearestEnergyStorageWithSpace(Position position)
    {
        // Priority on Spawns
        var fillableSpawn = _room.Find<IStructureSpawn>()
            .Where(x => x.Exists)
            .Where(x => x.Store[ResourceType.Energy] < x.Store.GetCapacity(ResourceType.Energy))
            .MinBy(x => x.LocalPosition.LinearDistanceTo(position));

        if (fillableSpawn != null)
        {
            return fillableSpawn;
        }
        
        return _room.Find<IStructure>()
            .Where(x => x.Exists && x is IStructureExtension)
            .Where(x => ((IStructureExtension)x).Store[ResourceType.Energy] < ((IStructureExtension)x).Store.GetCapacity(ResourceType.Energy))
            .MinBy(x => x.LocalPosition.LinearDistanceTo(position));
    }
}