using Unity.Plastic.Newtonsoft.Json;
namespace DrboumLibrary.Serialization {

    [JsonObject(ItemTypeNameHandling = TypeNameHandling.None)]
    public interface IUnityObjectReference {
        [JsonProperty]
        uint UObjectID { get; set; }
        void Initialize();
    }
}