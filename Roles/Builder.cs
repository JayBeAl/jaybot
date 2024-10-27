using System.Linq;
using Screeps.Roles.Components;
using ScreepsDotNet.API.World;

namespace Screeps.Roles;

public class Builder : RoleBase
{    
    private const string IdleFlagName = "Idle";
    
    private readonly EnergyReceivingComponent _energyReceivingComponent;
    private readonly IdleComponent _idleComponent;

    public Builder(IRoom room) : base(room)
    {
        _energyReceivingComponent = new EnergyReceivingComponent(_room);
        _idleComponent = new IdleComponent(_room);
    }
    
    public override void Run(ICreep creep)
    {       
        if (!_energyReceivingComponent.Tick(creep))
        {
            return;
        }

        if (ExecuteBuildingBehavior(creep))
        {
            return;
        }
        
        _idleComponent.Tick(creep, IdleFlagName);
    }

    private bool ExecuteBuildingBehavior(ICreep creep)
    {
        if(creep.Room!.Find<IConstructionSite>().Any())
        {
            var constructionSite = creep.Room!.Find<IConstructionSite>().First();
            if (creep.Build(constructionSite) == CreepBuildResult.NotInRange)
            {
                creep.MoveTo(constructionSite.LocalPosition);
            }

            return true;
        }

        return false;
    }
}