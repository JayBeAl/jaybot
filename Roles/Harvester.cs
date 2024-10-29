using System.Linq;
using Screeps.Manager.Source;
using Screeps.Roles.Components;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace Screeps.Roles;

public class Harvester : RoleBase
{
    private readonly SourceManager _sourceManager;
    
    private readonly IdleComponent _idleComponent;

    public Harvester(IRoom room, SourceManager sourceManager) : base(room)
    {
        _sourceManager = sourceManager;
        
        _idleComponent = new IdleComponent(room);
    }
    
    public override void Run(ICreep creep)
    {
        if (ExecuteHarvesterBehavior(creep))
        {
            return;
        }
        
        _idleComponent.Tick(creep, "Idle Harvester");
    }

    public override void OnDead(ICreep creep)
    {
        _sourceManager.LeaveEnergySource(creep);
    }

    private bool ExecuteHarvesterBehavior(ICreep creep)
    {
        if (!creep.Memory.TryGetBool("isMining", out var isMining))
        {
            creep.Memory.SetValue("isMining", false);
            creep.Memory.TryGetBool("isMining", out isMining);
        }
        
        if (!isMining && creep.Store.GetUsedCapacity() == 0)
        {
            isMining = true;
            creep.Say("Mining \u26a1");
        }
        
        if (isMining && creep.Store.GetFreeCapacity() == 0)
        {
            isMining = false;
            _sourceManager.LeaveEnergySource(creep);
            creep.Say("Delivering \ud83d\udea7");
        }
        
        if (isMining)
        {
            var assignedSource = _sourceManager.GetFreeEnergySource(creep);
            if (assignedSource != null)
            {
                if (creep.Harvest(assignedSource.Source) == CreepHarvestResult.NotInRange)
                {
                    creep.MoveTo(assignedSource.Source.LocalPosition);
                }

                creep.Memory.SetValue("isMining", isMining);
                return true;
            }
            
            creep.Say("No source");
            creep.Memory.SetValue("isMining", isMining);
            return false;
        }
        else
        {
            var storage = FindNearestEnergyStorageWithSpace(creep.LocalPosition);
            if (storage != null)
            {
                if (creep.Transfer(storage, ResourceType.Energy) == CreepTransferResult.NotInRange)
                {
                    creep.MoveTo(storage.RoomPosition);
                }
                
                creep.Memory.SetValue("isMining", isMining);
                return true;
            }

            creep.Say("No storage");
            creep.Memory.SetValue("isMining", isMining);
            return false;
        }
    }
    
    private IStructure? FindNearestEnergyStorageWithSpace(Position position)
    {
        var availableStorages = Room.Find<IStructure>().Where(structure => structure is IWithStore storage && storage.Store[ResourceType.Energy] < storage.Store.GetCapacity(ResourceType.Energy)).ToList();
        if (availableStorages.Count == 0)
        {
            return null;
        }
        
        var availableSpecificStorages = availableStorages.Where(storage => storage is IStructureSpawn).ToList();
        if (availableSpecificStorages.Count != 0)
        {
            return availableSpecificStorages.MinBy(x => x.LocalPosition.LinearDistanceTo(position));;
        }
        
        availableSpecificStorages = availableStorages.Where(storage => storage is IStructureContainer).ToList();
        if (availableSpecificStorages.Count != 0)
        {
            return availableSpecificStorages.MinBy(x => x.LocalPosition.LinearDistanceTo(position));;
        }
        
        availableSpecificStorages = availableStorages.Where(storage => storage is IStructureExtension).ToList();
        if (availableSpecificStorages.Count != 0)
        {
            return availableSpecificStorages.MinBy(x => x.LocalPosition.LinearDistanceTo(position));;
        }

        return null;
    }
}