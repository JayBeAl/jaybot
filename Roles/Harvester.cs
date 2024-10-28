using System;
using System.Collections.Generic;
using System.Linq;
using Screeps.Extensions;
using Screeps.Manager.Source;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace Screeps.Roles;

public class Harvester : RoleBase
{
    private readonly Dictionary<string, ISource> _sources = [];
    private readonly Dictionary<ICreep, string> _creepSources = [];

    public Harvester(IRoom room) : base(room)
    {
        var sources = room.Find<ISource>().ToList();
        foreach (var source in sources)
        {
            _sources.Add(source.Id.ToString(), source);
        }
    }
    
    public override void Run(ICreep creep)
    {
        if (!creep.Memory.TryGetBool("isMining", out var isMining))
        {
            creep.Memory.SetValue("isMining", false);
            creep.Memory.TryGetBool("isMining", out isMining);
        }
        
        if (!isMining && creep.Store.GetUsedCapacity() == 0)
        {
            if (AssignSource(creep))
            {
                isMining = true;
                creep.Say("Mining \u26a1");
            }
        }
        
        if (isMining && creep.Store.GetFreeCapacity() == 0)
        {
            if (UnassignSource(creep))
            {
                isMining = false;
                creep.Say("Delivering \ud83d\udea7");
            }
        }
        
        if (isMining)
        {
            if (creep.Memory.TryGetString("EnergySource", out var energySource) && energySource != string.Empty)
            {
                if (creep.Harvest(_sources[energySource]) == CreepHarvestResult.NotInRange)
                {
                    creep.MoveTo(_sources[energySource].LocalPosition);
                }
            }
        }
        else
        {
            var storage = FindNearestEnergyStorageWithSpace(creep.LocalPosition);
            if (storage == null)
            {
                creep.Say("No energy storage available.");
                return;
            }
            
            if (creep.Transfer(storage, ResourceType.Energy) == CreepTransferResult.NotInRange)
            {
                creep.MoveTo(storage.RoomPosition);
            }
        }
        
        creep.Memory.SetValue("isMining", isMining);
    }

    public void OnDead(ICreep creep)
    {
        if(_creepSources.Remove(creep, out var creepEnergySource) &&
           _sources[creepEnergySource].Memory(Room).TryGetInt(SourceProperty.ReservedSlots.ToString(), out var reservedSlots) &&
           reservedSlots > 0)
        {
            _sources[creepEnergySource].Memory(Room).SetValue(SourceProperty.ReservedSlots.ToString(), reservedSlots - 1);
        }
    }

    private bool AssignSource(ICreep creep)
    {
        if (!creep.Memory.TryGetString("EnergySource", out var energySource))
        {
            creep.Memory.SetValue("EnergySource", string.Empty);
        }

        if (energySource != string.Empty)
        {
            return false;
        }
        
        Console.WriteLine("Saved energy source: " + energySource);
        
        var sourceId = FindNearestFreeSource(creep.LocalPosition);
        Console.WriteLine("Found: " + sourceId);
        if (sourceId != null &&
            _sources[sourceId].Memory(Room).TryGetInt(SourceProperty.ReservedSlots.ToString(), out var reservedSlots) &&
            _sources[sourceId].Memory(Room).TryGetInt(SourceProperty.MiningSlots.ToString(), out var miningSlots) &&
            reservedSlots < miningSlots)
        {
            _sources[sourceId].Memory(Room).SetValue(SourceProperty.ReservedSlots.ToString(), reservedSlots + 1);
            _creepSources[creep] = sourceId;
            creep.Memory.SetValue("EnergySource", sourceId.ToString());
            return true;
        }

        return false;
    }

    private bool UnassignSource(ICreep creep)
    {
        if (creep.Memory.TryGetString("EnergySource", out var energySource) &&
            energySource != string.Empty &&
            _sources[energySource].Memory(Room).TryGetInt(SourceProperty.ReservedSlots.ToString(), out var reservedSlots) &&
            reservedSlots > 0)
        {
            _sources[energySource].Memory(Room).SetValue(SourceProperty.ReservedSlots.ToString(), reservedSlots - 1);
            _creepSources.Remove(creep);
            creep.Memory.SetValue("EnergySource", string.Empty);
            return true;
        }

        return false;
    }

    private ObjectId? FindNearestFreeSource(Position position)
    {
        var freeSources = new List<ISource>();
        foreach (var source in _sources.Values)
        {
            if (source.Memory(Room).TryGetInt(SourceProperty.ReservedSlots.ToString(), out var reservedSlots) &&
                source.Memory(Room).TryGetInt(SourceProperty.MiningSlots.ToString(), out var miningSlots) &&
                reservedSlots < miningSlots)
            {
                freeSources.Add(source);
            }
        }
        
        return freeSources.Count != 0 ? freeSources.MinBy(x => x.LocalPosition.LinearDistanceTo(position))?.Id : null;
    }
    
    private IStructure? FindNearestEnergyStorageWithSpace(Position position)
    {
        var availableStorages = Room.Find<IStructure>().Where(structure => structure is IWithStore storage && storage.Store[ResourceType.Energy] < storage.Store.GetCapacity(ResourceType.Energy)).ToList();
        if (availableStorages.Count == 0)
        {
            return null;
        }
        
        var availableSpecificStorages = availableStorages.Where(storage => storage is IStructureSpawn).ToList();
        if (availableSpecificStorages.Count != 0)
        {
            return availableSpecificStorages.MinBy(x => x.LocalPosition.LinearDistanceTo(position));;
        }
        
        availableSpecificStorages = availableStorages.Where(storage => storage is IStructureContainer).ToList();
        if (availableSpecificStorages.Count != 0)
        {
            return availableSpecificStorages.MinBy(x => x.LocalPosition.LinearDistanceTo(position));;
        }
        
        availableSpecificStorages = availableStorages.Where(storage => storage is IStructureExtension).ToList();
        if (availableSpecificStorages.Count != 0)
        {
            return availableSpecificStorages.MinBy(x => x.LocalPosition.LinearDistanceTo(position));;
        }

        return null;
    }
}