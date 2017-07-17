#region License

/*
 * Copyright © 2013-2016 the original author or authors.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Proteus.AppMessageBus.Abstractions;

namespace Proteus.AppMessageBus
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