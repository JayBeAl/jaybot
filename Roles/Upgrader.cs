using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace Screeps.Roles;

public class Upgrader(IRoom room) : RoleBase(room)
{
    public override void Run(ICreep creep)
    {
        if (!creep.Memory.TryGetBool("isUpgrading", out var isUpgrading))
        {
            creep.Memory.SetValue("isUpgrading", false);
            creep.Memory.TryGetBool("isUpgrading", out isUpgrading);
        }
        
        if (isUpgrading && creep.Store.GetUsedCapacity() == 0)
        {
            isUpgrading = false;
            creep.Say("Get \u26a1");
        }
        
        if (!isUpgrading && creep.Store.GetFreeCapacity() == 0)
        {
            isUpgrading = true;
            creep.Say("Upgrade \ud83d\udea7");
        }
        
        if (!isUpgrading && creep.Store.GetFreeCapacity() > 0)
        {
            var energyStorage = FindNearestFilledEnergyStorage(creep.LocalPosition);
            if (energyStorage == null)
            {
                return;
            }
            
            if (creep.Withdraw(energyStorage, ResourceType.Energy) == CreepWithdrawResult.NotInRange)
            {
                creep.MoveTo(energyStorage.LocalPosition);
            }
        }
        else if (isUpgrading && creep.Store.GetUsedCapacity() > 0)
        {
            if(creep.UpgradeController(creep.Room!.Controller!) == CreepUpgradeControllerResult.NotInRange)
            {
                creep.MoveTo(creep.Room!.Controller!.LocalPosition);
            }
        }
        else
        {
            // Go idle
            creep.Say("Idle \ud83d\udd04");
        }
        
        creep.Memory.SetValue("isUpgrading", isUpgrading);
    }

    private IStructure? FindNearestFilledEnergyStorage(Position position)
    {
        return _room.Find<IStructure>()
            .Where(x => x.Exists && (x is IStructureSpawn && ((IStructureSpawn)x).Store[ResourceType.Energy] > 0 
                                     || x is IStructureExtension && ((IStructureExtension)x).Store[ResourceType.Energy] > 0))
            .MinBy(x => x.LocalPosition.LinearDistanceTo(position));
    }
}