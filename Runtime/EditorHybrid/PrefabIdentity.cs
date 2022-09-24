using System.IO;
using Drboum.Utilities.Runtime.Attributes;
using Unity.Properties;
using UnityEditor;
using UnityEngine;

namespace Drboum.Utilities.Runtime.EditorHybrid
{
    public class PrefabIdentity : MonoBehaviour,IAssetReferenceID
    {
        [SerializeField, InspectorReadOnly, CreateProperty,InspectorName("PrefabGuid")] internal GuidWrapper _guid;
        
        public GuidWrapper Guid {
            get => _guid;
            internal  set => _guid = value;
        }

        GuidWrapper IAssetReferenceID.Guid {
            get => this.Guid;
            set => this.Guid = value;
        }

        public bool IsValidAsset => _guid.IsValid;
        
     
    }
}