using System.Collections.Generic;
using System.Linq;
using ScreepsDotNet.API.World;

namespace Screeps.Manager;

public class TowerManager
{
    private readonly IRoom _room;
    
    private readonly List<IStructureTower> _towers;
    private readonly List<IStructureWall> _walls;
    
    public TowerManager(IRoom room)
    {
        _room = room;

        _towers = InitiateTowers();
        _walls = _room.Find<IStructureWall>().ToList();
    }

    private List<IStructureTower> InitiateTowers()
    {
        return _room.Find<IStructureTower>().Where(tower => tower.My).ToList();
    }

    public void Tick()
    {
        var enemyCreeps = _room.Find<ICreep>().Where(creep => !creep.My).ToList();
        if (enemyCreeps.Count > 0)
        {
            foreach (var tower in _towers)
            {
                var attackTarget = enemyCreeps.MinBy(creep => creep.LocalPosition.CartesianDistanceTo(tower.LocalPosition));
                if (attackTarget == null)
                {
                    continue;
                }
            
                tower.Attack(attackTarget);
            }
        }
        else
        {
            foreach (var tower in _towers)
            {
                if (tower.Store.GetUsedCapacity(ResourceType.Energy) > tower.Store.GetCapacity(ResourceType.Energy) * 0.1f)
                {
                    var repairTarget = _walls.Where(wall => wall.HitsMax * 0.0001 > wall.Hits).MinBy(wall => wall.Hits);
                    if (repairTarget == null)
                    {
                        continue;
                    }

                    tower.Repair(repairTarget);
                }
            }
        }
    }
}