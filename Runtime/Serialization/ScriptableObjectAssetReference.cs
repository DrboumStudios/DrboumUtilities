using DrboumLibrary.Attributes;
using UnityEngine;
namespace DrboumLibrary.Serialization {
    public class ScriptableObjectAssetReference : ScriptableObject, IUnityObjectReference {
        [SerializeField] [InspectorReadOnly] private uint uObjectID;
        public                                       uint UObjectID { get => uObjectID; set => uObjectID = value; }

        public virtual void Initialize()
        { }
    }
}