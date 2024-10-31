using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Screeps.Extensions;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace Screeps.Manager.Source;

public class SourceManager
{
    private const int EnergyMinedPerTickAndWorkPart = 2;
    private const int RegenerationTicks = 300;
    
    private readonly IRoom _room;
    
    private readonly Dictionary<ICreep, WrappedSource> _sourceAssignments = new();

    public List<WrappedSource> Sources { get; set; } = [];
    
    public SourceManager(IRoom room)
    {
        _room = room;
        InstantiateSources();
    }

    public WrappedSource? GetFreeEnergySource(ICreep creep)
    {
        if (_sourceAssignments.TryGetValue(creep, out var value))
        {
            return value;
        }

        var creepWorkSize = creep.Body.Count(bodyPart => bodyPart.Type == BodyPartType.Work);
        
        var idealWrappedSource = Sources.Where(source => source.ReservedSlots < source.MiningSlots
                                                         // && source.CurrentWorkingParts + creepWorkSize <= source.MaxWorkingParts
                                                         )
            .MinBy(source => source.Source.LocalPosition.CartesianDistanceTo(creep.LocalPosition));
        
        if (idealWrappedSource != null)
        {
            idealWrappedSource.ReservedSlots++;
            // idealWrappedSource.CurrentWorkingParts += creep.Body.Count(bodyPart => bodyPart.Type == BodyPartType.Work);
            _sourceAssignments.Add(creep, idealWrappedSource);
            return idealWrappedSource;
        }

        return null;
    }

    public void LeaveEnergySource(ICreep creep)
    {
        if (_sourceAssignments.TryGetValue(creep, out var value))
        {
            value.ReservedSlots--;
            // value.CurrentWorkingParts -= creep.Body.Count(bodyPart => bodyPart.Type == BodyPartType.Work);
            _sourceAssignments.Remove(creep);
            return;
        }
    }

    private void InstantiateSources()
    {
        foreach (var source in _room.Find<ISource>())
        {
            if (!source.Memory().TryGetInt(SourceProperty.MiningSlots.ToString(), out var miningSlots))
            {
                miningSlots = GetFreeSlots(source.LocalPosition, 1).Count;
                source.Memory().SetValue(SourceProperty.MiningSlots.ToString(), miningSlots);
            }
            
            if (!source.Memory().TryGetString(SourceProperty.ContainerPosition.ToString(), out var containerPosition))
            {
                containerPosition = DetermineContainerPosition(source).ToString();
                source.Memory().SetValue(SourceProperty.ContainerPosition.ToString(), containerPosition);
            }
            
            if (!source.Memory().TryGetInt(SourceProperty.MaxWorkingParts.ToString(), out var maxWorkingParts))
            {
                maxWorkingParts = DetermineMaxWorkingParts(source);
                source.Memory().SetValue(SourceProperty.MaxWorkingParts.ToString(), maxWorkingParts);
            }
            
            var positionStringArray = containerPosition.Replace("[", "").Replace("]", "").Split(',');
            var position = new Position(int.Parse(positionStringArray[0]), int.Parse(positionStringArray[1]));

            var wrappedSource = new WrappedSource()
            {
                Source = source,
                MiningSlots = miningSlots,
                ReservedSlots = 0,
                ContainerPosition = position,
                MaxWorkingParts = maxWorkingParts,
                CurrentWorkingParts = 0
            };
            
            Sources.Add(wrappedSource);

            Console.WriteLine("Added Source: " + wrappedSource);
        }
    }

    private List<Position> GetFreeSlots(Position position, int radius, bool onlyBorder = false)
    {
        var freePositions = new List<Position>();

        var topLeft = new Position(position.X - radius, position.Y - radius);
        var bottomRight = new Position(position.X + radius, position.Y + radius);
        
        for (var y = topLeft.Y; y <= bottomRight.Y; y++)
        {
            for (var x = topLeft.X; x <= bottomRight.X; x++)
            {
                var currentPosition = new Position(x, y);
                if (IsTerrainBuildable(currentPosition))
                {
                    freePositions.Add(currentPosition);
                }
            }
        }

        if (onlyBorder)
        {
            foreach (var freePosition in freePositions.ToImmutableArray())
            {
                if (freePosition.X != topLeft.X &&
                    freePosition.Y != topLeft.Y &&
                    freePosition.X != bottomRight.X &&
                    freePosition.Y != bottomRight.Y)
                {
                    freePositions.Remove(freePosition);
                    Console.WriteLine($"Removed {freePosition}");
                }
            }
        }
        
        return freePositions;
    }

    private Position DetermineContainerPosition(ISource source)
    {
        var borderFreeSlots = GetFreeSlots(source.LocalPosition, 2, true);
        foreach (var borderFreeSlot in borderFreeSlots.ToImmutableArray())
        {
            if (GetFreeSlots(borderFreeSlot, 1).Count != 9)
            {
                borderFreeSlots.Remove(borderFreeSlot);
            }
        }
        
        return borderFreeSlots.MinBy(slot => slot.CartesianDistanceTo(source.LocalPosition));
    }

    private int DetermineMaxWorkingParts(ISource source)
    {
        return (source.EnergyCapacity / RegenerationTicks) / EnergyMinedPerTickAndWorkPart;
    }

    private bool IsTerrainBuildable(Position position)
    {
        if((_room.GetTerrain()[position] & Terrain.Wall) == 0)
        {
            return true;
        }

        return false;
    }
}