using ScreepsDotNet.API.World;

namespace Screeps.Roles;

public abstract class RoleBase(IRoom room) : IRole
{
    protected readonly IRoom Room = room;

    public abstract void Run(ICreep creep);
    
    public abstract void OnDead(ICreep creep);
}