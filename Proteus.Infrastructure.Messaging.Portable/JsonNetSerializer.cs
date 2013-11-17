using System.IO;
using System.Text;
using Newtonsoft.Json;
using Proteus.Infrastructure.Messaging.Portable.Abstractions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    public class JsonNetSerializer : ISerializer
    {
        public Stream Serialize<TSource>(TSource source)
        {
            var serializedString = JsonConvert.SerializeObject(source);
            return new MemoryStream(Encoding.UTF8.GetBytes(serializedString));
        }

        public TTarget Deserialize<TTarget>(Stream serialized)
        {
            var reader = new StreamReader(serialized);
            var streamAsString = reader.ReadToEnd();

            return JsonConvert.DeserializeObject<TTarget>(streamAsString);
        }
    }
}