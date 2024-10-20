using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace Screeps.Roles;

public class Upgrader(IRoom room) : RoleBase(room)
{
    private bool _isUpgrading;
    public override void Run(ICreep creep)
    {
        if (_isUpgrading && creep.Store.GetUsedCapacity() == 0)
        {
            _isUpgrading = false;
            creep.Say("Get \u26a1");
        }
        
        if (!_isUpgrading && creep.Store.GetFreeCapacity() == 0)
        {
            _isUpgrading = true;
            creep.Say("Upgrade \ud83d\udea7");
        }
        
        if (!_isUpgrading && creep.Store.GetFreeCapacity() > 0)
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
        else if (_isUpgrading && creep.Store.GetUsedCapacity() > 0)
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
    }

    private IStructure? FindNearestFilledEnergyStorage(Position position)
    {
        return _room.Find<IStructure>()
            .Where(x => x.Exists && (x is IStructureSpawn && ((IStructureSpawn)x).Store[ResourceType.Energy] > 0 
                                     || x is IStructureExtension && ((IStructureExtension)x).Store[ResourceType.Energy] > 0))
            .MinBy(x => x.LocalPosition.LinearDistanceTo(position));
    }
}