using UnityEngine;
namespace Drboum.Utilities.Runtime.Interfaces {
    public interface IHandleGameObjectLifeCycle : IEnableInstance, IDisableInstance {
        GameObject GameObjectRef { get; }

        void AddReusableInstance(IReusableInstance instance);
    }
}