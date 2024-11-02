using System.Linq;
using Screeps.Roles.Components;
using ScreepsDotNet.API.World;

namespace Screeps.Roles;

public class Hauler : RoleBase
{
    private const int CarryingCapacityPerBodyPart = 50;
    
    private readonly IdleComponent _idleComponent;

    public Hauler(IRoom room) : base(room)
    {
        
        _idleComponent = new IdleComponent(room);
    }
    
    public override void Run(ICreep creep)
    {
        if (ExecuteHaulerBehavior(creep))
        {
            return;
        }
        
        _idleComponent.Tick(creep, "Idle");
    }

    public override void OnDead(ICreep creep)
    {
        // Nothing to find here yet
    }

    private bool ExecuteHaulerBehavior(ICreep creep)
    {
        if (!creep.Memory.TryGetBool("IsWorking", out var isWorking))
        {
            creep.Memory.SetValue("IsWorking", false);
            creep.Memory.TryGetBool("IsWorking", out isWorking);
        }
        
        if (isWorking && creep.Store.GetUsedCapacity() == 0)
        {
            isWorking = false;
            creep.Say("Get \u26a1");
        }
        
        if (!isWorking && creep.Store.GetFreeCapacity() == 0)
        {
            isWorking = true;
            creep.Say("Delivering \u26a1");
        }

        if (isWorking)
        {
            var receiver = FindDemandingTowerOrExtension(creep);
            if (receiver != null)
            {
                if (creep.Transfer(receiver, ResourceType.Energy) == CreepTransferResult.NotInRange)
                {
                    creep.MoveTo(receiver.LocalPosition);
                }
                
                creep.Memory.SetValue("IsWorking", isWorking);
                return true;
            }
        }
        else
        {
            var storage = FindFilledContainer(creep);
            if (storage != null)
            {
                if (creep.Withdraw(storage, ResourceType.Energy) == CreepWithdrawResult.NotInRange)
                {
                    creep.MoveTo(storage.RoomPosition);
                }

                creep.Memory.SetValue("IsWorking", isWorking);
                return true;
            }
        }

        creep.Memory.SetValue("IsWorking", isWorking);
        return false;
    }

    private IStructureContainer? FindFilledContainer(ICreep creep)
    {
        return Room.Find<IStructureContainer>()
            .Where(container => container.Store.GetUsedCapacity() > creep.Body.Count(part => part.Type == BodyPartType.Carry) * CarryingCapacityPerBodyPart)
            .MinBy(container => container.LocalPosition.CartesianDistanceTo(creep.LocalPosition));
    }

    private IStructure? FindDemandingTowerOrExtension(ICreep creep)
    {
        var demandingTower = Room.Find<IStructureTower>()
            .Where(tower => tower.Store.GetFreeCapacity(ResourceType.Energy) > 0)
            .MinBy(tower => tower.LocalPosition.CartesianDistanceTo(creep.LocalPosition));

        if (demandingTower != null)
        {
            return demandingTower;
        }
        
        var demandingSpawn = Room.Find<IStructureSpawn>()
            .Where(spawn => spawn.Store.GetFreeCapacity(ResourceType.Energy) > 0)
            .MinBy(spawn => spawn.LocalPosition.CartesianDistanceTo(creep.LocalPosition));
        
        if (demandingSpawn != null)
        {
            return demandingSpawn;
        }
        
        var demandingExtension = Room.Find<IStructureExtension>()
            .Where(extension => extension.Store.GetFreeCapacity(ResourceType.Energy) > 0)
            .MinBy(extension => extension.LocalPosition.CartesianDistanceTo(creep.LocalPosition));
        
        if (demandingExtension != null)
        {
            return demandingExtension;
        }

        return null;
    }
}