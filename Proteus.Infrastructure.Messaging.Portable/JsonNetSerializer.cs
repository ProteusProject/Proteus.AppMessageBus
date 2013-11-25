using System.IO;
using System.Text;
using Newtonsoft.Json;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class JsonNetSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };

        public Stream Serialize<TSource>(TSource source)
        {
            var serializedString = JsonConvert.SerializeObject(source, _serializerSettings);
            return new MemoryStream(Encoding.UTF8.GetBytes(serializedString));
        }

        public TTarget Deserialize<TTarget>(Stream serialized)
        {
            var reader = new StreamReader(serialized);
            var serializedString = reader.ReadToEnd();

            return JsonConvert.DeserializeObject<TTarget>(serializedString, _serializerSettings);
        }
    }
}