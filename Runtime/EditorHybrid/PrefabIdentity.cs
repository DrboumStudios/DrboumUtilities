using Drboum.Utilities.Runtime.Attributes;
using UnityEngine;

namespace Drboum.Utilities.Runtime.EditorHybrid
{
    public class PrefabIdentity : MonoBehaviour, IAssetReferenceID
    {
        [SerializeField, InspectorReadOnly] internal GuidWrapper _guid;

        public GuidWrapper Guid {
            get => _guid;
            protected set => _guid = value;
        }

        GuidWrapper IAssetReferenceID.Guid {
            get => _guid;
            set => _guid = value;
        }

        public bool IsValidAsset => _guid.IsValid;


    }
}