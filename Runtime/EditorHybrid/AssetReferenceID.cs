using Unity.Mathematics;
using UnityEngine;
namespace Drboum.Utilities.Runtime.EditorHybrid {
    public interface IAssetReferenceID {
        GuidWrapper Guid {
            get;
        }
        bool IsValidAsset {
            get;
        }
    }
    
    public abstract class AssetReferenceID : EditorCallBackScriptableObject<AssetReferenceID>, IAssetReferenceID {
        [SerializeField] [HideInInspector] protected string _guid;
        [SerializeField] [HideInInspector] internal  int   instanceId;
        internal                                     bool  _skipDuplication;

        internal       bool IsValidGuid  => !_guid.Equals(default);
        public virtual bool IsValidAsset => IsValidGuid;
        public GuidWrapper Guid {
            get => _guid;
            internal set => _guid = value.GuidValue.ToString("N");
        }
#if UNITY_EDITOR
        [ContextMenu(nameof(PrintGUIDAsGuidWrapper))]
        internal void PrintGUIDAsGuidWrapper()
        {
            LogHelper.LogInfoTypedMessage(Guid, $"{name}");
        }
#endif
    }
}