using System.IO;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface ISerializer
    {
        Stream Serialize<TSource>(TSource source);
        TTarget Deserialize<TTarget>(Stream serialized);
    }
}