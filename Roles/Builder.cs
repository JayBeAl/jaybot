using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace Screeps.Roles;

public class Builder(IRoom room) : RoleBase(room)
{
    private bool _isBuilding;
    
    public override void Run(ICreep creep)
    {
        if (_isBuilding && creep.Store.GetUsedCapacity() == 0)
        {
            _isBuilding = false;
            creep.Say("Get \u26a1");
        }

        if (!_isBuilding && creep.Store.GetFreeCapacity() == 0)
        {
            _isBuilding = true;
            creep.Say("Build \ud83d\udea7");
        }
        
        if (!_isBuilding && creep.Store.GetFreeCapacity() > 0)
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
        else if(_isBuilding && creep.Room!.Find<IConstructionSite>().Any())
        {
            var constructionSite = creep.Room!.Find<IConstructionSite>().First();
            if (creep.Build(constructionSite) == CreepBuildResult.NotInRange)
            {
                creep.MoveTo(constructionSite.LocalPosition);
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