namespace Drboum.Utilities.Runtime.Interfaces {
    public interface IInitializable<T> {
        void Initialize(T initializationParameter);
    }
    public interface IInitializable {
        void Initialize();
    }
}