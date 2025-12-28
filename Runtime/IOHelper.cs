using System.Buffers;
using System.IO;
using System.Threading.Tasks;
using Unity.Collections;

namespace Drboum.Utilities
{
    public static class IOHelper
    {
        public static async Task WriteToFileAsync(NativeArray<byte> nativeStream, string filePath)
        {
            int fileStreamLength = nativeStream.Length;
            var bufferManagedArray = ArrayPool<byte>.Shared.Rent(fileStreamLength);
            NativeArray<byte>.Copy(nativeStream, bufferManagedArray, fileStreamLength);
            using var fileStream = File.OpenWrite(filePath);
            await fileStream.WriteAsync(bufferManagedArray, 0, fileStreamLength);
            fileStream.Flush(true);
            ArrayPool<byte>.Shared.Return(bufferManagedArray);
        }
    }
}