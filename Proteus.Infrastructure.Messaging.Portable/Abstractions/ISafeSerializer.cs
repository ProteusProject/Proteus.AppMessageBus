using System.IO;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    interface ISafeSerializer
    {
        bool TrySerializeToStream<TSource>(TSource source, out Stream serialized);
        bool TrySerializeToString<TSource>(TSource source, out string serialized);
        bool TryDeserialize<TTarget>(Stream serialized, out TTarget obj);
        bool TryDeserialize<TTarget>(string serialized, out TTarget obj); 
    }
}