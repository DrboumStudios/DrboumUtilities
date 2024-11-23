using System;
using Unity.Entities;

namespace Drboum.Utilities.Entities.Baking
{
    /// <summary>
    /// Identity for baker that spawns entities
    /// </summary>
    public struct OriginBakerIdentity : IComponentData,IEquatable<OriginBakerIdentity>
    {
        public int Value;

        public OriginBakerSharedKey ToSharedKey()
        {
            return new OriginBakerSharedKey {
                Value = Value
            };
        }

        public bool Equals(OriginBakerIdentity other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is OriginBakerIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(OriginBakerIdentity left, OriginBakerIdentity right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OriginBakerIdentity left, OriginBakerIdentity right)
        {
            return !left.Equals(right);
        }
    }
    
    /// <summary>
    /// Identify and group the entities using their baker's Id
    /// </summary>
    [BakingType]
    public struct OriginBakerSharedKey : ISharedComponentData, IEquatable<OriginBakerSharedKey>
    {
        public int Value;

        public bool Equals(OriginBakerSharedKey other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is OriginBakerSharedKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(OriginBakerSharedKey left, OriginBakerSharedKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OriginBakerSharedKey left, OriginBakerSharedKey right)
        {
            return !left.Equals(right);
        }

        public static implicit operator OriginBakerSharedKey(OriginBakerIdentity originBakerIdentity)
        {
            return new OriginBakerSharedKey {
                Value = originBakerIdentity.Value
            };
        }
    }
}