using System.Diagnostics;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;
namespace Drboum.Utilities.Runtime.Collections {
    /// <summary>
    ///     fixed buffer of untyped memory
    /// </summary>
    /// <typeparam name="TBufferSize"></typeparam>
    public unsafe struct FixedUntypedBuffer<TBufferSize> where TBufferSize : unmanaged {
        public static readonly int MaxCapacity = sizeof(TBufferSize) > ushort.MaxValue ? ushort.MaxValue : sizeof(TBufferSize);

        private ushort _length;
        public void AddData<T>(T data, out ushort memoryId) where T : unmanaged
        {
            int sizeOfNode = sizeof(T);
            AllocateData(sizeOfNode, out memoryId);
            SetData(ref data, memoryId);
        }
        public void AllocateData(int sizeOfData, out ushort memoryId)
        {
            int newLength = _length + sizeOfData;
            AssertDoesNotOverflow(newLength);
            memoryId = _length;
            _length = (ushort)newLength;
        }
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void AssertDoesNotOverflow(int newLength)
        {
            Assert.IsTrue(newLength <= MaxCapacity, "BufferOverflow");
        }
        public ref T GetData<T>(ushort memoryId) where T : unmanaged
        {
            byte* ptr = GetBufferPtr(memoryId);
            return ref UnsafeUtility.AsRef<T>(ptr);
        }
        public void SetData<T>(ref T data, ushort memoryId) where T : unmanaged
        {
            byte* ptr = GetBufferPtr(memoryId);
            UnsafeUtility.CopyStructureToPtr(ref data, ptr);
        }
        private byte* GetBufferPtr(ushort offset = 0)
        {
            return (byte*)UnsafeUtility.AddressOf(ref _dataBuffer) + offset;
        }

        private TBufferSize _dataBuffer;
    }
}