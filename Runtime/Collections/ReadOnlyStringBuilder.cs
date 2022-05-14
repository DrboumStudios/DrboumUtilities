using System.Text;
namespace DrboumLibrary {
    public class ReadOnlyStringBuilder {
        private readonly StringBuilder _stringBuilder;

        public ReadOnlyStringBuilder(StringBuilder stringBuilder)
        {
            _stringBuilder = stringBuilder;
        }
        public void Append(string text)
        {
            _stringBuilder.Append(text);
        }
    }
}