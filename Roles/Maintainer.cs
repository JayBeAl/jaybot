using System;
using System.Linq;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace Screeps.Roles;

public class Maintainer(IRoom room) : RoleBase(room)
{
    private const float RepairThreshold = 0.3f;
    private const float RepairFinishedThreshold = 0.95f;
    
    public override void Run(ICreep creep)
    {
        if (!creep.Memory.TryGetBool("isMaintaining", out var isMaintaining))
        {
            creep.Memory.SetValue("isMaintaining", false);
            creep.Memory.TryGetBool("isMaintaining", out isMaintaining);
        }
        
        if (!creep.Memory.TryGetBool("hasFullEnergy", out var hasFullEnergy))
        {
            creep.Memory.SetValue("hasFullEnergy", false);
            creep.Memory.TryGetBool("hasFullEnergy", out hasFullEnergy);
        }
        
        if (hasFullEnergy && creep.Store.GetUsedCapacity() == 0)
        {
            hasFullEnergy = false;
            creep.Say("Get \u26a1");
        }

        if (!hasFullEnergy && creep.Store.GetFreeCapacity() == 0)
        {
            hasFullEnergy = true;
            creep.Say("Repair \ud83d\udea7");
        }

        if (!isMaintaining)
        {
            var building = FindBuildingToMaintain(creep);
            Console.WriteLine("Building found: " + building);
            if (building != null)
            {
                creep.SetUserData(building);
                isMaintaining = true;
            }
        }

        if (isMaintaining)
        {
            var building = creep.GetUserData<IStructure>();
            if (building != null)
            {
                if ((float)building.Hits / building.HitsMax > RepairFinishedThreshold)
                {
                    isMaintaining = false;
                }

                if (!hasFullEnergy)
                {
                    var energyStorage = FindNearestFilledEnergyStorage(creep.LocalPosition);
                    if (energyStorage != null)
                    {
                        if (creep.Withdraw(energyStorage, ResourceType.Energy) == CreepWithdrawResult.NotInRange)
                        {
                            creep.MoveTo(energyStorage.LocalPosition);
                        }
                    }
                }
                else
                {
                    if (creep.Repair(building) == CreepRepairResult.NotInRange)
                    {
                        creep.MoveTo(building.LocalPosition);
                    }
                }
            }
            else
            {
                isMaintaining = false;
            }
        }
        else
        {
            creep.Say("Idle \ud83d\udd04");
        }
        
        creep.Memory.SetValue("isMaintaining", isMaintaining);
        creep.Memory.SetValue("hasFullEnergy", hasFullEnergy);
    }

    private IStructure? FindBuildingToMaintain(ICreep creep)
    {
        var buildingToMaintain = _room.Find<IStructure>()
            .Where(building => (float)building.Hits / building.HitsMax < RepairThreshold && building is not IStructureRoad or IStructureWall)
            .OrderBy(building => creep.LocalPosition.LinearDistanceTo(building.LocalPosition));;
        
        if (buildingToMaintain.Any())
        {
            return buildingToMaintain.First();
        }
        
         buildingToMaintain = _room.Find<IStructureRoad>()
             .Where(building => (float)building.Hits / building.HitsMax < RepairThreshold)
             .OrderBy(building => creep.LocalPosition.LinearDistanceTo(building.LocalPosition));

         if (buildingToMaintain.Any())
         {
             return buildingToMaintain.First();
         }
         
         buildingToMaintain = _room.Find<IStructureWall>()
             .Where(building => (float)building.Hits / building.HitsMax < RepairThreshold)
             .OrderBy(building => creep.LocalPosition.LinearDistanceTo(building.LocalPosition));
         
         if (buildingToMaintain.Any())
         {
             return buildingToMaintain.First();
         }

         return null;
    }

    private IStructure? FindNearestFilledEnergyStorage(Position position)
    {
        return _room.Find<IStructure>()
            .Where(x => x.Exists && (x is IStructureSpawn && ((IStructureSpawn)x).Store[ResourceType.Energy] > 0 
                                     || x is IStructureExtension && ((IStructureExtension)x).Store[ResourceType.Energy] > 0))
            .MinBy(x => x.LocalPosition.LinearDistanceTo(position));
    }
}