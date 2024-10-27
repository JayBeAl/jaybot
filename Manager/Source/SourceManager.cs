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
    private readonly IRoom _room;
    
    public SourceManager(IRoom room)
    {
        _room = room;
        InstantiateSources();
    }

    private void InstantiateSources()
    {
        foreach (var source in _room.Find<ISource>())
        {
            if (!source.Memory(_room).TryGetInt(SourceProperty.MiningSlots.ToString(), out var miningSlots))
            {
                miningSlots = GetFreeSlots(source.LocalPosition, 1).Count;
                source.Memory(_room).SetValue(SourceProperty.MiningSlots.ToString(), miningSlots);
            }
            
            if (!source.Memory(_room).TryGetInt(SourceProperty.ReservedSlots.ToString(), out var reservedSlots))
            {
                reservedSlots = 0;
                source.Memory(_room).SetValue(SourceProperty.ReservedSlots.ToString(), reservedSlots);
            }
            
            if (!source.Memory(_room).TryGetString(SourceProperty.ContainerPosition.ToString(), out var containerPosition))
            {
                containerPosition = DetermineContainerPosition(source).ToString();
                source.Memory(_room).SetValue(SourceProperty.ContainerPosition.ToString(), containerPosition);
            }
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

    private bool IsTerrainBuildable(Position position)
    {
        if((_room.GetTerrain()[position] & Terrain.Wall) == 0)
        {
            return true;
        }

        return false;
    }
}