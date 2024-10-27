using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace Screeps.Roles.Components;

public class EnergyReceivingComponent
{
    private readonly IRoom _room;
    
    public EnergyReceivingComponent(IRoom room)
    {
        _room = room;
    }

    public bool Tick(ICreep creep)
    {
        if (!creep.Memory.TryGetBool("hasEnergy", out var hasEnergy))
        {
            creep.Memory.SetValue("hasEnergy", false);
            creep.Memory.TryGetBool("hasEnergy", out hasEnergy);
        }

        if (!hasEnergy && creep.Store.GetFreeCapacity() == 0)
        {
            hasEnergy = true;
            creep.Say("Full \u26a1");
        }

        if (hasEnergy && creep.Store.GetUsedCapacity() == 0)
        {
            hasEnergy = false;
            creep.Say("Get \u26a1");
        }

        if (!hasEnergy)
        {
            var energyStorage = FindNearestFilledEnergyStorage(creep.LocalPosition);
            if (energyStorage == null)
            {
                creep.Memory.SetValue("hasEnergy", hasEnergy);
                return false;
            }
            
            if (creep.Withdraw(energyStorage, ResourceType.Energy) == CreepWithdrawResult.NotInRange)
            {
                creep.MoveTo(energyStorage.LocalPosition);
            }
            
            creep.Memory.SetValue("hasEnergy", hasEnergy);
            return false;
        }
        
        
        creep.Memory.SetValue("hasEnergy", hasEnergy);
        return true;
    }
    
    private IStructure? FindNearestFilledEnergyStorage(Position position)
    {
        return _room.Find<IStructure>()
            .Where(x => x.Exists && (x is IStructureSpawn spawn && spawn.Store[ResourceType.Energy] > 0 
                                     || x is IStructureExtension extension && extension.Store[ResourceType.Energy] > 0))
            .MinBy(x => x.LocalPosition.LinearDistanceTo(position));
    }
}