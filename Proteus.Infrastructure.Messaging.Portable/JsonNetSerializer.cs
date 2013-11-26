using System.IO;
using System.Text;
using Newtonsoft.Json;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class JsonNetSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };

        public Stream SerializeToStream<TSource>(TSource source)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(SerializeToString(source)));
        }

        public string SerializeToString<TSource>(TSource source)
        {
            return JsonConvert.SerializeObject(source, _serializerSettings);
        }

        public TTarget Deserialize<TTarget>(Stream serialized)
        {
            var reader = new StreamReader(serialized);
            var serializedString = reader.ReadToEnd();

            return Deserialize<TTarget>(serializedString);
        }

        public TTarget Deserialize<TTarget>(string serialized)
        {
            return JsonConvert.DeserializeObject<TTarget>(serialized, _serializerSettings);
        }
    }
}