using UnityEngine;
namespace DrboumLibrary.Interfaces {
    public interface IHandleGameObjectLifeCycle : IEnableInstance, IDisableInstance {
        GameObject GameObjectRef { get; }

        void AddReusableInstance(IReusableInstance instance);
    }
}