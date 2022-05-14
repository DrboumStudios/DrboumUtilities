using System.IO;
using System.Text;
using Unity.Plastic.Newtonsoft.Json;
namespace DrboumLibrary.Serialization {
    public class JsonManager {
        private readonly JsonTextWriter _jsonStringWriter;

        private readonly StringBuilder _stringBuilder;
        private readonly StringWriter  _stringWriter;

        public JsonManager(JsonSerializerSettings settings)
        {
            _stringBuilder    = new StringBuilder();
            _stringWriter     = new StringWriter(_stringBuilder);
            _jsonStringWriter = new JsonTextWriter(_stringWriter);
            JsonSerializer    = JsonSerializer.Create(settings);
        }

        public JsonSerializer JsonSerializer { get; }

        public string ToJsonString<T>(T data)
            where T : class
        {
            _stringBuilder.Clear();

            JsonSerializer.Serialize(_jsonStringWriter, data, typeof(T));
            var str = _stringBuilder.ToString();
            return str;

        }
        public void FromJsonString<T>(string jsonString, T dataBuffer)
            where T : class
        {
            using var strReader = new StringReader(jsonString);
            using var reader    = new JsonTextReader(strReader);
            JsonSerializer.Populate(reader, dataBuffer);
        }
        public void WriteJsonToFile<T>(T data, string fullPath)
            where T : class
        {

            using var strWriter = new StreamWriter(fullPath, false, Encoding.ASCII);
            using var writer    = new JsonTextWriter(strWriter);
            JsonSerializer.Serialize(writer, data, typeof(T));
        }
        public void LoadFromJsonFile<T>(T data, string fullPath)
            where T : class
        {
            using var strReader = new StreamReader(fullPath, Encoding.ASCII);
            using var reader    = new JsonTextReader(strReader);
            JsonSerializer.Populate(reader, data);
        }
    }
}