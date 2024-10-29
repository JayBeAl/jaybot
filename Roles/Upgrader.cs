using Screeps.Roles.Components;
using ScreepsDotNet.API.World;

namespace Screeps.Roles;

public class Upgrader : RoleBase
{
    private const int ControllerMaxLevel = 8;
    private const string IdleFlagName = "Idle";
    
    private readonly EnergyReceivingComponent _energyReceivingComponent;
    private readonly IdleComponent _idleComponent;

    public Upgrader(IRoom room) : base(room)
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

        if (ExecuteUpgraderBehavior(creep))
        {
            return;
        }

        _idleComponent.Tick(creep, IdleFlagName);
    }

    public override void OnDead(ICreep creep)
    {
        // Nothing to find here yet
    }

    private bool ExecuteUpgraderBehavior(ICreep creep)
    {
        if (creep.Room!.Controller!.Level != ControllerMaxLevel)
        {
            if (creep.UpgradeController(creep.Room!.Controller!) == CreepUpgradeControllerResult.NotInRange)
            {
                creep.MoveTo(creep.Room!.Controller!.LocalPosition);
            }

            return true;
        }

        return false;
    }
}