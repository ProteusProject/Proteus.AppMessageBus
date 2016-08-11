using System.IO;

namespace Proteus.AppMessageBus.Portable.Abstractions
{
    public interface ISerializer
    {
        Stream SerializeToStream<TSource>(TSource source);
        string SerializeToString<TSource>(TSource source);
        TTarget Deserialize<TTarget>(Stream serialized);
        TTarget Deserialize<TTarget>(string serialized);
    }
}