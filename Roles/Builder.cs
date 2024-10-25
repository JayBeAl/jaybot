using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace Screeps.Roles;

public class Builder(IRoom room) : RoleBase(room)
{
    public override void Run(ICreep creep)
    {
        if (!creep.Memory.TryGetBool("isBuilding", out var isBuilding))
        {
            creep.Memory.SetValue("isBuilding", false);
            creep.Memory.TryGetBool("isBuilding", out isBuilding);
        }
        
        if (isBuilding && creep.Store.GetUsedCapacity() == 0)
        {
            isBuilding = false;
            creep.Say("Get \u26a1");
        }

        if (!isBuilding && creep.Store.GetFreeCapacity() == 0)
        {
            isBuilding = true;
            creep.Say("Build \ud83d\udea7");
        }
        
        if (!isBuilding && creep.Store.GetFreeCapacity() > 0)
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
        else if(isBuilding && creep.Room!.Find<IConstructionSite>().Any())
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
        
        creep.Memory.SetValue("isBuilding", isBuilding);
    }
    
    private IStructure? FindNearestFilledEnergyStorage(Position position)
    {
        return _room.Find<IStructure>()
            .Where(x => x.Exists && (x is IStructureSpawn && ((IStructureSpawn)x).Store[ResourceType.Energy] > 0 
                                     || x is IStructureExtension && ((IStructureExtension)x).Store[ResourceType.Energy] > 0))
            .MinBy(x => x.LocalPosition.LinearDistanceTo(position));
    }
}