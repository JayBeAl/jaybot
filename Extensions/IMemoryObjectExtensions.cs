using ScreepsDotNet.API.World;

namespace Screeps.Extensions;

public static class MemoryObjectExtensions
{
    internal static IMemoryObject Memory(this ISource source, IRoom room)
    {
        return room.Memory.GetOrCreateObject("sources").GetOrCreateObject(source.Id);
    }
}