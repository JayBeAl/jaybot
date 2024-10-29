using System.Linq;
using Screeps.Roles.Components;
using ScreepsDotNet.API.World;

namespace Screeps.Roles;

public class Maintainer : RoleBase
{
    private const float RepairThreshold = 0.3f;
    private const float RepairFinishedThreshold = 0.95f;
    private const string IdleFlagName = "Idle";
    
    private readonly EnergyReceivingComponent _energyReceivingComponent;
    private readonly IdleComponent _idleComponent;

    public Maintainer(IRoom room) : base(room)
    {
        _energyReceivingComponent = new EnergyReceivingComponent(Room);
        _idleComponent = new IdleComponent(Room);
    }
    
    public override void Run(ICreep creep)
    {
        if (!_energyReceivingComponent.Tick(creep))
        {
            return;
        }

        if (ExecuteMaintainerBehavior(creep))
        {
            return;
        }
        
        _idleComponent.Tick(creep, IdleFlagName);
    }

    public override void OnDead(ICreep creep)
    {
        // Nothing to find here yet
    }

    private bool ExecuteMaintainerBehavior(ICreep creep)
    {
        if (!creep.Memory.TryGetBool("isMaintaining", out var isMaintaining))
        {
            creep.Memory.SetValue("isMaintaining", false);
            creep.Memory.TryGetBool("isMaintaining", out isMaintaining);
        }

        if (!isMaintaining)
        {
            var building = FindBuildingToMaintain(creep);
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
                else if(creep.Repair(building) == CreepRepairResult.NotInRange)
                {
                    creep.MoveTo(building.LocalPosition);
                }
            }
            else
            {
                isMaintaining = false;
            }
        }
        
        creep.Memory.SetValue("isMaintaining", isMaintaining);
        return isMaintaining;
    }

    private IStructure? FindBuildingToMaintain(ICreep creep)
    {
        var buildingToMaintain = Room.Find<IStructure>()
            .Where(building => (float)building.Hits / building.HitsMax < RepairThreshold && building is not IStructureRoad or IStructureWall)
            .OrderBy(building => creep.LocalPosition.LinearDistanceTo(building.LocalPosition));;
        
        if (buildingToMaintain.Any())
        {
            return buildingToMaintain.First();
        }
        
        buildingToMaintain = Room.Find<IStructureRoad>()
             .Where(building => (float)building.Hits / building.HitsMax < RepairThreshold)
             .OrderBy(building => creep.LocalPosition.LinearDistanceTo(building.LocalPosition));

         if (buildingToMaintain.Any())
         {
             return buildingToMaintain.First();
         }
         
         buildingToMaintain = Room.Find<IStructureWall>()
             .Where(building => (float)building.Hits / building.HitsMax < RepairThreshold)
             .OrderBy(building => creep.LocalPosition.LinearDistanceTo(building.LocalPosition));
         
         if (buildingToMaintain.Any())
         {
             return buildingToMaintain.First();
         }

         return null;
    }
}