﻿using System;
using Drboum.Utilities.Runtime.Attributes;
using Unity.Mathematics;
using UnityEngine;

namespace Drboum.Utilities.Runtime.EditorHybrid
{
    public interface IAssetReferenceID
    {
        GuidWrapper Guid {
            get;
            internal set;
        }
        bool IsValidAsset {
            get;
        }
    }

    public abstract class AssetReferenceID : EditorCallBackScriptableObject<AssetReferenceID>, IAssetReferenceID, IEquatable<AssetReferenceID>
    {
        public const string ASSET_REFERENCE_EXTENSION = ".asset";

        [SerializeField, InspectorReadOnly] internal GuidWrapper _guid;

        internal bool IsValidGuid => _guid.IsValid;
        public virtual bool IsValidAsset => IsValidGuid;

        public GuidWrapper Guid {
            get => _guid;
            internal  set => _guid = value;
        }

        GuidWrapper IAssetReferenceID.Guid {
            get => this.Guid;
            set => this.Guid = value;
        }

        public bool Equals(AssetReferenceID other)
        {
            if ( other is null )
            {
                return false;
            }
            if ( ReferenceEquals(this, other) )
            {
                return true;
            }
            return _guid == other._guid;
        }

        public override bool Equals(object obj) => obj is AssetReferenceID assetReferenceID && Equals(assetReferenceID);

        public override int GetHashCode() => Guid.GetHashCode();
    }
}