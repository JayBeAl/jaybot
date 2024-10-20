using ScreepsDotNet.API.World;

namespace Screeps.Roles;

public abstract class RoleBase(IRoom room) : IRole
{
    protected readonly IRoom _room = room;

    public abstract void Run(ICreep creep);
}