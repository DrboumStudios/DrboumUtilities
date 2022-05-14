using Unity.Collections;
namespace DrboumLibrary.Interfaces {
    public interface IProvideFixedStringTypeInfo {
        public ref readonly FixedString64Bytes TypeName { get; }
    }
}