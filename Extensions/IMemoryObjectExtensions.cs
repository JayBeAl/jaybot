using ScreepsDotNet.API.World;

namespace Screeps.Extensions;

public static class MemoryObjectExtensions
{
    internal static IMemoryObject Memory(this ISource source)
    {
        return source.Room.Memory.GetOrCreateObject("sources").GetOrCreateObject(source.Id);
    }
}