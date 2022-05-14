using Unity.Plastic.Newtonsoft.Json;
namespace DrboumLibrary.Serialization {
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.None)]
    public interface IRegisteredType {
        [JsonProperty(PropertyName = SerializationTypeManager.TypeIDJsonPropertyName)]
        int TypeId { get; }
    }
}