using Unity.Collections;
namespace Drboum.Utilities.Runtime.Interfaces {
    public interface IProvideFixedStringTypeInfo {
        public ref readonly FixedString64Bytes TypeName { get; }
    }
}