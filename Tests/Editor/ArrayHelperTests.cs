using DrboumLibrary;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
namespace DrboumLibrary.Tests {

    public class ArrayHelperTests {
        [Test]
        public void FlatNativeArray2DGetSetValue()
        {
            const int expectedValue = 15;
            const int count         = 5;
            var       array         = new NativeArray2D<int>(count, count, Allocator.Temp);
            array[2, 4] = expectedValue;
            int arrayValue = array[2, 4];
            Assert.AreEqual(expectedValue, arrayValue);
        }
        [Test]
        public void FlatNativeArray2DFillArrayAndGetExpectedValues()
        {
            var array = new NativeArray2D<int>(4, 10, Allocator.Temp);
            for ( var i = 0; i < array.DimensionOneLength; i++ ) {
                for ( var ii = 0; ii < array.DimensionTwoLength; ii++ ) {
                    array[i, ii] = array.GetFlatIndex(i, ii);
                }
            }
            Debug.Log($"{nameof(FlatNativeArray2DFillArrayAndGetExpectedValues)} {array}");

            Assert.AreEqual(0, array[0, 0]);
            Assert.AreEqual(1, array[0, 1]);
            int maxExpectedValue = array.GetFlatIndex(array.DimensionOneLength - 1, array.DimensionTwoLength - 1);
            Assert.AreEqual(maxExpectedValue, array[array.DimensionOneLength   - 1, array.DimensionTwoLength - 1]);
        }

        [Test]
        public void FlatNativeArray2DCheckIfAllValueAreValid()
        {
            var array = new NativeArray2D<int>(4, 10, Allocator.Temp);
            InitializeAllArrayData(array);
            Assert.AreEqual(0, array._flatNativeArray[0]);
            for ( var i = 1; i < array._flatNativeArray.Length; i++ ) {
                Assert.AreNotEqual(0, array._flatNativeArray[i]);
            }
        }
        [Test]
        public void FlatNativeArray3DGetSetValue()
        {
            const int expectedValue = 15;
            const int count         = 5;
            var       array         = new NativeArray3D<int>(count, count, count, Allocator.Temp);
            array[2, 4, 2] = expectedValue;
            int arrayValue = array[2, 4, 2];
            Assert.AreEqual(expectedValue, arrayValue);
        }
        [Test]
        public void FlatNativeArray3DFillArrayAndGetExpectedValues()
        {
            var array = new NativeArray3D<int>(2, 10, 15, Allocator.Temp);
            InitializeAllArrayData(array);
            Debug.Log($"{nameof(FlatNativeArray3DFillArrayAndGetExpectedValues)} {array}");
            Assert.AreEqual(0,                           array[0, 0, 0]);
            Assert.AreEqual(1,                           array[0, 0, 1]);
            Assert.AreEqual(array.GetFlatIndex(1, 1, 1), array[1, 1, 1]);

            int maxExpectedValue = array.GetFlatIndex(array.DimensionOneLength - 1, array.DimensionTwoLength - 1, array.DimensionThreeLength - 1);
            Assert.AreEqual(maxExpectedValue, array[array.DimensionOneLength   - 1, array.DimensionTwoLength - 1, array.DimensionThreeLength - 1]);
        }

        [Test]
        public void FlatNativeArray3DCheckIfAllValueAreValid()
        {
            var array = new NativeArray3D<int>(10, 10, 10, Allocator.Temp);
            InitializeAllArrayData(array);
            Assert.AreEqual(0, array._flatNativeArray[0]);
            for ( var i = 1; i < array._flatNativeArray.Length; i++ ) {
                Assert.AreEqual(i, array._flatNativeArray[i]);
            }
        }

        private static void InitializeAllArrayData(NativeArray3D<int> array)
        {
            for ( var i = 0; i < array.DimensionOneLength; i++ ) {
                for ( var ii = 0; ii < array.DimensionTwoLength; ii++ ) {
                    for ( var iii = 0; iii < array.DimensionThreeLength; iii++ ) {
                        array[i, ii, iii] = array.GetFlatIndex(i, ii, iii);
                    }
                }
            }
        }
        private static void InitializeAllArrayData(NativeArray2D<int> array)
        {
            for ( var i = 0; i < array.DimensionOneLength; i++ ) {
                for ( var ii = 0; ii < array.DimensionTwoLength; ii++ ) {
                    array[i, ii] = array.GetFlatIndex(i, ii);
                }
            }
        }
    }

}
public class CollectionExtTests {
    private const string _waypointMapFormat = "test" + "successfullymapped: id {0} of length {1}";
    [Test]
    public void BytesAsFixedStringZeroTestOutput()
    {
        byte i = 0;
        Assert.AreEqual(0.ToString(), i.ToFixedStringAsByte<FixedString32Bytes, byte>().ToString());
    }
    [Test]
    public unsafe void Bytes16ToFixedStringIsComplete()
    {
        byte               maxValue          = 255;
        int                charCountPerValue = maxValue.ToString().Length;
        var                buffer            = CreateBytesForType<GuidWrapper>(maxValue);
        int                sizeOf            = sizeof(GuidWrapper);
        int                countPerValue     = charCountPerValue * sizeOf;
        int                separatorCount    = sizeOf         - 1;
        int                total             = separatorCount + countPerValue;
        var fstring           = buffer.ToFixedString();

        LogHelper.LogInfoMessage(fstring.ToString(), "TEST");
        Assert.AreEqual(total, fstring.Length);
    }
    private static unsafe T CreateBytesForType<T>()
        where T : unmanaged
    {
        T id = default;

        var ptr = (byte*)UnsafeUtility.AddressOf(ref id);
        for ( byte i = 0; i < sizeof(T); i++ ) {
            ptr[i] = i;
        }
        return id;
    }
    private static unsafe T CreateBytesForType<T>(byte fixedValue)
        where T : unmanaged
    {
        T content = default;

        var ptr = (byte*)UnsafeUtility.AddressOf(ref content);
        for ( byte i = 0; i < sizeof(T); i++ ) {
            ptr[i] = fixedValue;
        }
        return content;
    }
}