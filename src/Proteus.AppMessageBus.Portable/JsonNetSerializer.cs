﻿using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Proteus.AppMessageBus.Portable.Abstractions;

namespace Proteus.AppMessageBus.Portable
{
    public class JsonNetSerializer : IMessageSerializer
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

        public SerializerResult<Stream> TrySerializeToStream<TSource>(TSource source)
        {
            try
            {
                return new SerializerResult<Stream>(SerializeToStream(source), true);
            }
            catch (Exception ex)
            {
                return new SerializerResult<Stream>(null, false, ex);
            }
        }

        public SerializerResult<string> TrySerializeToString<TSource>(TSource source)
        {
            try
            {
                return new SerializerResult<string>(SerializeToString(source), true);
            }
            catch (Exception ex)
            {
                return new SerializerResult<string>(null, false, ex);
            }
        }

        public SerializerResult<TTarget> TryDeserialize<TTarget>(Stream serialized)
        {
            try
            {
                var deserialized = Deserialize<TTarget>(serialized);

                return ReferenceEquals(deserialized, null) ?
                    new SerializerResult<TTarget>(default(TTarget), false, new SerializationException("Deserialize Result is NULL.")) 
                    : new SerializerResult<TTarget>(deserialized, true);
            }
            catch (Exception ex)
            {
                return new SerializerResult<TTarget>(default(TTarget), false, ex);
            }
        }

        public SerializerResult<TTarget> TryDeserialize<TTarget>(string serialized)
        {
            try
            {
                var deserialized = Deserialize<TTarget>(serialized);
                return ReferenceEquals(deserialized, null) ?
                    new SerializerResult<TTarget>(default(TTarget), false, new SerializationException("Deserialize Result is NULL."))
                    : new SerializerResult<TTarget>(deserialized, true);
            }
            catch (Exception ex)
            {
                return new SerializerResult<TTarget>(default(TTarget), false, ex);
            }
        }
    }
}