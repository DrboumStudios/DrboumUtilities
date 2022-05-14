using System;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
namespace DrboumLibrary.Serialization {
    public class JsonTypeDeserializer<T> : JsonConverter
        where T : class, IRegisteredType {

        public          string IdPropertyName { get; set; }
        public override bool   CanWrite       => false;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer                         serializer)
        {
            JObject obj         = JObject.Load(reader);
            JToken  typeIdToken = obj.SelectToken(IdPropertyName);
            if ( typeIdToken == null ) {
                return null;
            }

            var typeId = Convert.ToInt32(typeIdToken.ToString());
            SerializationTypeManager.GetType(typeId, out Type type);
            return obj.ToObject(type, serializer) as T;
        }
    }
}