using Screeps.Extensions;
using ScreepsDotNet.API;
using ScreepsDotNet.API.World;

namespace Screeps.Manager.Source;

public class WrappedSource
{
    public ISource Source { get; init; } = default!;

    public int MiningSlots
    {
        get
        {
            if (!Source.Memory().TryGetInt(SourceProperty.MiningSlots.ToString(), out var miningSlots))
            {
                Source.Memory().SetValue(SourceProperty.MiningSlots.ToString(), 0);
                return 0;
            }

            return miningSlots;
        }
        init
        {
            if (value >= 0)
            {
                Source.Memory().SetValue(SourceProperty.MiningSlots.ToString(), value);
            }
        }
    }

    public int ReservedSlots
    {
        get
        {
            if (!Source.Memory().TryGetInt(SourceProperty.ReservedSlots.ToString(), out var reservedSlots))
            {
                Source.Memory().SetValue(SourceProperty.ReservedSlots.ToString(), 0);
                return 0;
            }

            return reservedSlots;
        }
        set
        {
            if (value >= 0 & value <= MiningSlots)
            {
                Source.Memory().SetValue(SourceProperty.ReservedSlots.ToString(), value);
            }
        }
    }    
    
    public int MaxWorkingParts
    {
        get
        {
            if (!Source.Memory().TryGetInt(SourceProperty.MaxWorkingParts.ToString(), out var maxWorkingParts))
            {
                Source.Memory().SetValue(SourceProperty.MaxWorkingParts.ToString(), 0);
                return 0;
            }

            return maxWorkingParts;
        }
        init
        {
            if (value >= 0)
            {
                Source.Memory().SetValue(SourceProperty.MaxWorkingParts.ToString(), value);
            }
        }
    }

    public int CurrentWorkingParts
    {
        get
        {
            if (!Source.Memory().TryGetInt(SourceProperty.CurrentWorkingParts.ToString(), out var currentWorkingParts))
            {
                Source.Memory().SetValue(SourceProperty.CurrentWorkingParts.ToString(), 0);
                return 0;
            }

            return currentWorkingParts;
        }
        set
        {
            if (value >= 0 & value <= MaxWorkingParts)
            {
                Source.Memory().SetValue(SourceProperty.CurrentWorkingParts.ToString(), value);
            }
        }
    }
    
    public Position ContainerPosition
    {
        get
        {
            if (!Source.Memory().TryGetString(SourceProperty.ContainerPosition.ToString(), out var containerPosition))
            {
                var newPosition = new Position(-1, -1);
                Source.Memory().SetValue(SourceProperty.ContainerPosition.ToString(), newPosition.ToString());
                return newPosition;
            }
            
            var positionStringArray = containerPosition.Replace("[", "").Replace("]", "").Split(',');
            var position = new Position(int.Parse(positionStringArray[0]), int.Parse(positionStringArray[1]));
            return position;
        }
        init => Source.Memory().SetValue(SourceProperty.ContainerPosition.ToString(), value.ToString());
    }

    public override string ToString()
    {
        return $"{Source.Id} - Max:{MiningSlots}-Reserved:{ReservedSlots}-Container:{ContainerPosition}";
    }
}