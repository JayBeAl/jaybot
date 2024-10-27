using System.Linq;
using ScreepsDotNet.API.World;

namespace Screeps.Roles.Components;

public class IdleComponent
{
    private readonly IRoom _room;
    
    public IdleComponent(IRoom room)
    {
        _room = room;
    }

    public void Tick(ICreep creep, string targetFlagName)
    {
        var targetFlag = _room.Find<IFlag>().First(flag => flag.Name == targetFlagName);
        creep.MoveTo(targetFlag.LocalPosition);
        creep.Say("Idle \ud83d\udd04");
    }
}