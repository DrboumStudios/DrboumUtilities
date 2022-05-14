using UnityEngine;
namespace DrboumLibrary.Serialization {

    public static class AssetReferenceRuntime {
        /// <summary>
        ///     resource folder relative path
        /// </summary>
        private const string _assetReferenceUObjectRegistryPath = "PersistenceSystem/_UnityObjectReferenceRegistry_";

        static AssetReferenceRuntime()
        {
            Registry = Resources.Load<UnityObjectReferenceRegistrySO>(_assetReferenceUObjectRegistryPath);
        }
        public static UnityObjectReferenceRegistrySO Registry {
            get;
        }
    }

}