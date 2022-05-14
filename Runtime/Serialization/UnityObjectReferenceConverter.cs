using System;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;
namespace DrboumLibrary.Serialization {
    public class UnityObjectReferenceConverter : JsonConverter {
        public override bool CanWrite => false;
        public override bool CanRead  => true;
        public override bool CanConvert(Type objectType)
        {
            return typeof(IUnityObjectReference).IsAssignableFrom(objectType);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer                         serializer)
        {
            IUnityObjectReference unityObjectReference = null;
            if ( reader.TokenType == JsonToken.StartObject ) {

                JObject jToken = JObject.Load(reader);
                if ( jToken == null ) {
                    Debug.LogError("the JObject could not be load with the reader");
                    return null;
                }
                JToken uObjectIDToken = jToken[nameof(IUnityObjectReference.UObjectID)];
                if ( uObjectIDToken == null ) {
#if DEBUG
                    Debug.LogError($"a key json {nameof(IUnityObjectReference.UObjectID)} could not found");
#endif
                    return null;
                }

                var id = Convert.ToUInt32(uObjectIDToken.ToString());

                UnityObjectReferenceRegistrySO.GetUnityObjectReference(id, out unityObjectReference);
            }
            return unityObjectReference;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        { }
    }
}