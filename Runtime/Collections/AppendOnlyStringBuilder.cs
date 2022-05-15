using System.Text;
namespace Drboum.Utilities.Runtime.Collections {
    public class AppendOnlyStringBuilder {
        private readonly StringBuilder _stringBuilder;

        public AppendOnlyStringBuilder(StringBuilder stringBuilder)
        {
            _stringBuilder = stringBuilder;
        }
        public void Append(string text)
        {
            _stringBuilder.Append(text);
        }
    }
}