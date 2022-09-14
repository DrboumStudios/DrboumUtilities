using System;
using Unity.Mathematics;
using UnityEngine;
namespace Drboum.Utilities.Runtime.EditorHybrid
{
    public interface IAssetReferenceID
    {
        GuidWrapper Guid {
            get;
        }
        bool IsValidAsset {
            get;
        }
    }

    public abstract class AssetReferenceID : EditorCallBackScriptableObject<AssetReferenceID>, IAssetReferenceID, IEquatable<AssetReferenceID>
    {
        [SerializeField] [HideInInspector] protected string _guid;
        [SerializeField] [HideInInspector] internal int instanceId;
        internal bool _skipDuplication;

        internal bool IsValidGuid => !string.IsNullOrEmpty(_guid) && !default(GuidWrapper).Equals(_guid);
        public virtual bool IsValidAsset => IsValidGuid;
        public GuidWrapper Guid {
            get => _guid;
            internal set => _guid = value.GuidValue.ToString("N");
        }

        public bool Equals(AssetReferenceID other)
        {
            if (other is null )
            {
                return false;
            }
            if (  ReferenceEquals(this, other) )
            {
                return true;
            }
            return _guid == other._guid;
        }
        
        public override bool Equals(object obj) => obj is AssetReferenceID assetReferenceID && Equals(assetReferenceID);

        public override int GetHashCode() => Guid.GetHashCode();
    }
}