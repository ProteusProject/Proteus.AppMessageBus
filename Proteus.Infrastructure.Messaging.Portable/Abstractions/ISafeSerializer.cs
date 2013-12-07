using System.IO;

namespace Proteus.Infrastructure.Messaging.Portable.Abstractions
{
    public interface ISafeSerializer
    {
        SerializerResult<Stream> TrySerializeToStream<TSource>(TSource source);
        SerializerResult<string> TrySerializeToString<TSource>(TSource source);
        SerializerResult<TTarget> TryDeserialize<TTarget>(Stream serialized);
        SerializerResult<TTarget> TryDeserialize<TTarget>(string serialized); 
    }
}