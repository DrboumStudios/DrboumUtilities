namespace DrboumLibrary.Interfaces {
    public interface IInitializable<T> {
        void Initialize(T initializationParameter);
    }
}