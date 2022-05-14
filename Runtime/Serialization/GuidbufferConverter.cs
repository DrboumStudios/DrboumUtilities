using System;
using DrboumLibrary.Animation;
using Unity.Plastic.Newtonsoft.Json;
namespace DrboumLibrary.Serialization {
    public class GuidWrapperConverter : JsonConverter<GuidWrapper> {

        public override GuidWrapper ReadJson(JsonReader reader,           Type           objectType, GuidWrapper existingValue,
            bool                                        hasExistingValue, JsonSerializer serializer)
        {
            if ( reader.Value == null ) {
                return existingValue;
            }

            existingValue.GuidValue = serializer.Deserialize<Guid>(reader);
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, GuidWrapper value, JsonSerializer serializer)
        {
            writer.WriteValue(value.GuidValue.ToString("n"));
        }
    }
    public class AnimatorParameterSerializedDataConverter : JsonConverter<AnimatorParameterValue> {

        public override AnimatorParameterValue ReadJson(JsonReader reader,        Type objectType,
            AnimatorParameterValue                                 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if ( reader.Value == null ) {
                return existingValue;
            }
            existingValue.ValueAsInt = Convert.ToInt32(reader.Value);
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, AnimatorParameterValue value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ValueAsInt);
        }
    }
}