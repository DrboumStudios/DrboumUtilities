namespace Drboum.Utilities.Runtime.Interfaces {
    public interface IConvertStruct<In, Out>
        where In : struct
        where Out : struct {

        Out Convert(In valueToConvert);
    }
}