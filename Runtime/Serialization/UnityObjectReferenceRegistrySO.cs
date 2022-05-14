using System.Collections.Generic;
using UnityEngine;
namespace DrboumLibrary.Serialization {
    [CreateAssetMenu(fileName = "NewUnityObjectReferenceRegistry",
        menuName = "Serialization/UnityObjectReferenceRegistry")]
    public class UnityObjectReferenceRegistrySO : ScriptableObject {
        public static readonly Dictionary<uint, IUnityObjectReference> ObjectIDMapping =
            new Dictionary<uint, IUnityObjectReference>();

        public List<Object> UnityObjectReferenceIDs = new List<Object>();

        public static bool GetUnityObjectReference<T>(uint objectID, out T unityObjectReference)
            where T : class, IUnityObjectReference
        {
            bool found = ObjectIDMapping.TryGetValue(objectID, out IUnityObjectReference unityRef);
            unityObjectReference = unityRef as T;
            return found;
        }

        public void ListToDictionary()
        {
            for ( var i = 0; i < UnityObjectReferenceIDs.Count; ++i ) {
                var unityObjectReferenceID = UnityObjectReferenceIDs[i] as IUnityObjectReference;
#if DEBUG
                Object @object = UnityObjectReferenceIDs[i];
#endif

                if ( unityObjectReferenceID.UObjectID == 0 ) {
#if DEBUG
                    Debug.LogError($"Object: {@object.name} does not have his unique Prefab ID generated");
#endif
                    continue;
                }

                if ( !ObjectIDMapping.ContainsKey(unityObjectReferenceID.UObjectID) ) {
                    unityObjectReferenceID.Initialize();
                    ObjectIDMapping.Add(unityObjectReferenceID.UObjectID, unityObjectReferenceID);
                }
            }
        }
    }
}