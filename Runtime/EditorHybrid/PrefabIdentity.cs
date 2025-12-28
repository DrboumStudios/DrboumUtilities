using Drboum.Utilities.Attributes;
using Unity.Properties;
using UnityEngine;

namespace Drboum.Utilities.EditorHybrid
{
    public class PrefabIdentity : MonoBehaviour, IAssetReferenceID, IAssetFactorySettings
    {
        [SerializeField, InspectorReadOnly] protected AssetFactorySettings _assetFactorySettings;
        [SerializeField, InspectorReadOnly, DontCreateProperty] internal GuidWrapper _guid;

        public GuidWrapper Guid {
            get => _guid;
            protected set => _guid = value;
        }

        GuidWrapper IAssetReferenceID.Guid {
            get => _guid;
            set => _guid = value;
        }

        public bool IsValid => _guid.IsValid;
        public AssetFactorySettings AssetFactorySettings => _assetFactorySettings;
        public virtual void OnCreateAsset() { }

        public virtual GuidWrapper GenerateGuid()
        {
            return IAssetReferenceID.GenerateGuidFromAsset(this);
        }

    }
}