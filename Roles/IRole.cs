using ScreepsDotNet.API.World;

namespace Screeps.Roles;

public interface IRole
{
    void Run(ICreep creep);
    void OnDead(ICreep creep);
}