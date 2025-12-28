using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace Drboum.Utilities
{
    public static class MathHelper
    {
        public const float FULL_ROTATION_RADIANS = math.PI * 2f;

        /// <summary>
        ///     the direction is assumed to be normalized
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="position"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDestinationInFrontOfDirection(float3 direction, float3 position, float3 destination)
        {
            float3 destDir = destination - position;
            float dot = math.dot(direction, destDir);
            return dot > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 CalculatePositionOffsetRelativeFrom(in quaternion relativeToRotation, in float3 offsetPosition)
        {
            return math.mul(relativeToRotation, offsetPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothStart(float t)
        {
            return t * t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 SmoothStart(float2 t)
        {
            return t * t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothStop(float t)
        {
            float mt = 1f - t;
            return 1f - mt * mt;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 SmoothStop(float2 t)
        {
            float2 mt = 1f - t;
            return 1f - mt * mt;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothStep(float t)
        {
            //return math.lerp(SmoothStart(t), SmoothStop(t), t);
            var t2 = t * t;
            return 3 * t2 - 2 * t2 * t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Divide(Vector3 vector3, Vector3 by)
        {
            return new Vector3(vector3.x / by.x, vector3.y / by.y, vector3.z / by.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 DivideSafe(Vector3 vector3, Vector3 by)
        {
            float resultx = CheckDivideByZero(vector3.x, by.x);
            float resulty = CheckDivideByZero(vector3.y, by.y);
            float resultz = CheckDivideByZero(vector3.z, by.z);

            return new Vector3(resultx, resulty, resultz);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CheckDivideByZero(float val1, float val2)
        {
            bool divideByZero = val2 == 0f;
#if UNITY_EDITOR
            if ( divideByZero )
            {
                Debug.LogError("Operation Trying to divide by 0");
            }
#endif

            return divideByZero ? val2 : val1 / val2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion RotateTowards(this quaternion from, quaternion to, float maxRadianDelta)
        {
            float dot = math.min(math.abs(math.dot(from, to)), 1.0f);
            float angle = dot > 1.0f - math.EPSILON ? 0.0f : math.acos(dot) * 2.0F;
            if ( angle == 0.0f )
            {
                return to;
            }
            return math.slerp(from, to, math.min(1.0f, maxRadianDelta / angle));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RotateAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            // we get point direction from the point to the pivot
            Vector3 direction = point - pivot;
            // we rotate the direction
            direction = Quaternion.Euler(angles) * direction;
            // we determine the rotated point's position
            point = direction + pivot;
            return point;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetHorizontalCircleEdgePosition(float y, Vector3 center, float horizontalAngle, float radius)
        {
            Vector3 pointResult = Vector3.zero;
            GetPointOnCircleEdge(ref pointResult, radius, horizontalAngle);
            pointResult.y = y;
            pointResult.x += center.x;
            pointResult.z += center.z;
            return pointResult;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetPointOnCircleEdge(ref Vector3 point, float radius, float horizontalAngle)
        {
            point.x = radius * math.sin(horizontalAngle);
            point.z = radius * math.cos(horizontalAngle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 RotateAroundPivot(Vector3 point, Vector3 pivot, Quaternion quaternion)
        {
            // we get point direction from the point to the pivot
            Vector3 direction = point - pivot;
            // we rotate the direction
            direction = quaternion * direction;
            // we determine the rotated point's position
            point = direction + pivot;
            return point;
        }

        public static Vector3 DirectionFromAngle(float angle, float additionalAngle)
        {
            angle += additionalAngle;

            Vector3 direction = Vector3.zero;
            direction.x = math.sin(angle * Mathf.Deg2Rad);
            direction.y = 0f;
            direction.z = math.cos(angle * Mathf.Deg2Rad);
            return direction;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlignToDirectionSmooth(Transform toRotate, Vector3 desiredDirection, float turnAngleSpeed)
        {
            float delta = math.degrees(math.acos(math.dot(toRotate.forward, desiredDirection)));
            Quaternion desiredRotation = Quaternion.LookRotation(desiredDirection);
            float angleDelta = Mathf.Abs(delta);
            if ( angleDelta > turnAngleSpeed && turnAngleSpeed > 0f )
            {
                float step = turnAngleSpeed / angleDelta;
                toRotate.rotation = Quaternion.LerpUnclamped(toRotate.rotation, desiredRotation, step);
                return false;
            }
            toRotate.rotation = desiredRotation;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AlignToDirectionHorizontalSmooth(Transform toRotate, Vector3 desiredDirection,
            float turnAngleSpeed)
        {
            desiredDirection.y = toRotate.forward.y;
            desiredDirection.Normalize();
            float dotVal = math.dot(toRotate.forward, desiredDirection);
            float delta = math.degrees(math.acos(dotVal));
            Quaternion desiredRotation = Quaternion.LookRotation(desiredDirection);
            float angleDelta = Mathf.Abs(delta);
            if ( angleDelta > turnAngleSpeed && turnAngleSpeed > 0f )
            {
                float step = turnAngleSpeed / angleDelta;
                toRotate.rotation = Quaternion.LerpUnclamped(toRotate.rotation, desiredRotation, step);
                return false;
            }
            toRotate.rotation = desiredRotation;
            return true;
        }

        /// <summary>
        ///     take the offsetSign as input and output the result in that variable
        /// </summary>
        /// <param name="number"></param>
        /// <param name="boolToByte"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BranchLessSign(ref float number, ref BoolToByte boolToByte)
        {
            boolToByte.Condition = number >= 0;
            number = (boolToByte.ConditionResultAsNumber + -0.5f) * 2;
        }

        /// <summary>
        ///     <see cref="BranchLessSign(ref float,ref MathHelper.BoolToByte)" />
        /// </summary>
        /// <param name="offsetSign"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BranchLessSign(ref float3 offsetSign)
        {
            BoolToByte boolToByte = default;
            BranchLessSign(ref offsetSign, ref boolToByte);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BranchLessSign(ref float3 offsetSign, ref BoolToByte boolToByte)
        {
            BranchLessSign(ref offsetSign.x, ref boolToByte);
            BranchLessSign(ref offsetSign.y, ref boolToByte);
            BranchLessSign(ref offsetSign.z, ref boolToByte);
        }

        /// <summary>
        ///     Convenience method for one shot
        /// </summary>
        /// <param name="number"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BranchLessSign(ref float number)
        {
            BoolToByte boolToByte = default;
            BranchLessSign(ref number, ref boolToByte);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sign(in float number, out float result)
        {
            result = number >= 0 ? 1f : -1f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDefault(this quaternion quaternion)
        {
            return quaternion.value.x == 0 && quaternion.value.y == 0 && quaternion.value.z == 0 && quaternion.value.w == 0;
        }

        /// <summary>
        ///     Timecounter must be between 0 and 1
        /// </summary>
        /// <param name="timeCounter"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Flatten(this float3 float3)
        {
            float3.y = 0f;
            return float3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 FlattenNormalize(this float3 float3)
        {
            float3.y = 0f;
            math.normalize(float3);
            return float3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 FlattenNormalizeSafe(this ref float3 float3ToNormalize)
        {
            float3ToNormalize.y = 0f;
            math.normalizesafe(float3ToNormalize);
            return float3ToNormalize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(this float3 from, float3 to)
        {
            return math.degrees(math.acos(math.dot(math.normalize(from), math.normalize(to))));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion FromToRotation(this quaternion from, quaternion to)
        {
            return math.mul(math.inverse(from), to);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngleSigned(this float3 from, float3 to)
        {
            float angle = math.acos(math.dot(math.normalize(from), math.normalize(to)));
            float3 cross = math.cross(from, to);
            angle *= math.sign(math.dot(math.up(), cross));
            return math.degrees(angle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ProjectOnPlane(this float3 vector, float3 onPlane)
        {
            float3 orthogonalComponent = onPlane * math.dot(vector, onPlane);
            return vector - orthogonalComponent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ProjectOnNormal(this float3 vector, float3 onNormal)
        {
            return onNormal * math.dot(vector, onNormal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ClampMagnitude(this float3 vector, float magnitude)
        {
            float lengthScale = math.length(vector) / magnitude;
            if ( lengthScale > 1f )
            {
                vector = vector * (1f / lengthScale);
            }
            return vector;
        }

        /// <summary>
        ///     Returns the Euler angles (in radians) between two quaternions
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 EstimateAnglesBetween(this quaternion from, quaternion to)
        {
            var fromImag = new float3(from.value.x, from.value.y, from.value.z);
            var toImag = new float3(to.value.x, to.value.y, to.value.z);

            float3 angle = math.cross(fromImag, toImag);
            angle -= to.value.w * fromImag;
            angle += from.value.w * toImag;
            angle += angle;
            return math.dot(toImag, fromImag) < 0 ? -angle : angle;
        }

        /// <summary>
        ///     Note: taken from Unity.Animation/Core/MathExtensions.cs, which will be moved to Unity.Mathematics at some point
        ///     after that, this should be removed and the Mathematics version should be used
        /// </summary>
        /// <param name="q"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ToEuler(this quaternion q, math.RotationOrder order = math.RotationOrder.Default)
        {
            const float epsilon = 1e-6f;

            //prepare the data
            float4 qv = q.value;
            float4 d1 = qv * qv.wwww * new float4(2.0f); //xw, yw, zw, ww
            float4 d2 = qv * qv.yzxw * new float4(2.0f); //xy, yz, zx, ww
            float4 d3 = qv * qv;
            var euler = new float3(0.0f);

            const float CUTOFF = (1.0f - 2.0f * epsilon) * (1.0f - 2.0f * epsilon);

            switch ( order )
            {
                case math.RotationOrder.ZYX:
                {
                    float y1 = d2.z + d1.y;
                    if ( y1 * y1 < CUTOFF )
                    {
                        float x1 = -d2.x + d1.z;
                        float x2 = d3.x + d3.w - d3.y - d3.z;
                        float z1 = -d2.y + d1.x;
                        float z2 = d3.z + d3.w - d3.y - d3.x;
                        euler = new float3(math.atan2(x1, x2), math.asin(y1), math.atan2(z1, z2));
                    }
                    else //zxz
                    {
                        y1 = math.clamp(y1, -1.0f, 1.0f);
                        var abcd = new float4(d2.z, d1.y, d2.y, d1.x);
                        float x1 = 2.0f * (abcd.x * abcd.w + abcd.y * abcd.z); //2(ad+bc)
                        float x2 = math.csum(abcd * abcd * new float4(-1.0f, 1.0f, -1.0f, 1.0f));
                        euler = new float3(math.atan2(x1, x2), math.asin(y1), 0.0f);
                    }

                    break;
                }

                case math.RotationOrder.ZXY:
                {
                    float y1 = d2.y - d1.x;
                    if ( y1 * y1 < CUTOFF )
                    {
                        float x1 = d2.x + d1.z;
                        float x2 = d3.y + d3.w - d3.x - d3.z;
                        float z1 = d2.z + d1.y;
                        float z2 = d3.z + d3.w - d3.x - d3.y;
                        euler = new float3(math.atan2(x1, x2), -math.asin(y1), math.atan2(z1, z2));
                    }
                    else //zxz
                    {
                        y1 = math.clamp(y1, -1.0f, 1.0f);
                        var abcd = new float4(d2.z, d1.y, d2.y, d1.x);
                        float x1 = 2.0f * (abcd.x * abcd.w + abcd.y * abcd.z); //2(ad+bc)
                        float x2 = math.csum(abcd * abcd * new float4(-1.0f, 1.0f, -1.0f, 1.0f));
                        euler = new float3(math.atan2(x1, x2), -math.asin(y1), 0.0f);
                    }

                    break;
                }

                case math.RotationOrder.YXZ:
                {
                    float y1 = d2.y + d1.x;
                    if ( y1 * y1 < CUTOFF )
                    {
                        float x1 = -d2.z + d1.y;
                        float x2 = d3.z + d3.w - d3.x - d3.y;
                        float z1 = -d2.x + d1.z;
                        float z2 = d3.y + d3.w - d3.z - d3.x;
                        euler = new float3(math.atan2(x1, x2), math.asin(y1), math.atan2(z1, z2));
                    }
                    else //yzy
                    {
                        y1 = math.clamp(y1, -1.0f, 1.0f);
                        var abcd = new float4(d2.x, d1.z, d2.y, d1.x);
                        float x1 = 2.0f * (abcd.x * abcd.w + abcd.y * abcd.z); //2(ad+bc)
                        float x2 = math.csum(abcd * abcd * new float4(-1.0f, 1.0f, -1.0f, 1.0f));
                        euler = new float3(math.atan2(x1, x2), math.asin(y1), 0.0f);
                    }

                    break;
                }

                case math.RotationOrder.YZX:
                {
                    float y1 = d2.x - d1.z;
                    if ( y1 * y1 < CUTOFF )
                    {
                        float x1 = d2.z + d1.y;
                        float x2 = d3.x + d3.w - d3.z - d3.y;
                        float z1 = d2.y + d1.x;
                        float z2 = d3.y + d3.w - d3.x - d3.z;
                        euler = new float3(math.atan2(x1, x2), -math.asin(y1), math.atan2(z1, z2));
                    }
                    else //yxy
                    {
                        y1 = math.clamp(y1, -1.0f, 1.0f);
                        var abcd = new float4(d2.x, d1.z, d2.y, d1.x);
                        float x1 = 2.0f * (abcd.x * abcd.w + abcd.y * abcd.z); //2(ad+bc)
                        float x2 = math.csum(abcd * abcd * new float4(-1.0f, 1.0f, -1.0f, 1.0f));
                        euler = new float3(math.atan2(x1, x2), -math.asin(y1), 0.0f);
                    }

                    break;
                }

                case math.RotationOrder.XZY:
                {
                    float y1 = d2.x + d1.z;
                    if ( y1 * y1 < CUTOFF )
                    {
                        float x1 = -d2.y + d1.x;
                        float x2 = d3.y + d3.w - d3.z - d3.x;
                        float z1 = -d2.z + d1.y;
                        float z2 = d3.x + d3.w - d3.y - d3.z;
                        euler = new float3(math.atan2(x1, x2), math.asin(y1), math.atan2(z1, z2));
                    }
                    else //xyx
                    {
                        y1 = math.clamp(y1, -1.0f, 1.0f);
                        var abcd = new float4(d2.x, d1.z, d2.z, d1.y);
                        float x1 = 2.0f * (abcd.x * abcd.w + abcd.y * abcd.z); //2(ad+bc)
                        float x2 = math.csum(abcd * abcd * new float4(-1.0f, 1.0f, -1.0f, 1.0f));
                        euler = new float3(math.atan2(x1, x2), math.asin(y1), 0.0f);
                    }

                    break;
                }

                case math.RotationOrder.XYZ:
                {
                    float y1 = d2.z - d1.y;
                    if ( y1 * y1 < CUTOFF )
                    {
                        float x1 = d2.y + d1.x;
                        float x2 = d3.z + d3.w - d3.y - d3.x;
                        float z1 = d2.x + d1.z;
                        float z2 = d3.x + d3.w - d3.y - d3.z;
                        euler = new float3(math.atan2(x1, x2), -math.asin(y1), math.atan2(z1, z2));
                    }
                    else //xzx
                    {
                        y1 = math.clamp(y1, -1.0f, 1.0f);
                        var abcd = new float4(d2.z, d1.y, d2.x, d1.z);
                        float x1 = 2.0f * (abcd.x * abcd.w + abcd.y * abcd.z); //2(ad+bc)
                        float x2 = math.csum(abcd * abcd * new float4(-1.0f, 1.0f, -1.0f, 1.0f));
                        euler = new float3(math.atan2(x1, x2), -math.asin(y1), 0.0f);
                    }

                    break;
                }
            }

            return EulerReorderBack(euler, order);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 EulerReorderBack(float3 euler, math.RotationOrder order)
        {
            switch ( order )
            {
                case math.RotationOrder.XZY:
                    return euler.xzy;
                case math.RotationOrder.YZX:
                    return euler.zxy;
                case math.RotationOrder.YXZ:
                    return euler.yxz;
                case math.RotationOrder.ZXY:
                    return euler.yzx;
                case math.RotationOrder.ZYX:
                    return euler.zyx;
                case math.RotationOrder.XYZ:
                default:
                    return euler;
            }
        }

        /// <summary>
        ///     get the offset between self and another vector
        /// </summary>
        /// <param name="self"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static Vector3 GetOffset(this Vector3 self, Vector3 other)
        {
            return other - self;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct BoolToByte
        {
            [FieldOffset(0)] public bool Condition;
            /// <summary>
            ///     0 or 1 depending on the assigned condition
            /// </summary>
            [FieldOffset(0)] public byte ConditionResultAsNumber;

            /// <summary>
            ///     this function will return 1 if the condition is true or else -1
            /// </summary>
            /// <param name="condition"></param>
            /// <param name="result"></param>
            public void ConvertToNormalizedSignedNumber(bool condition, out float result)
            {
                Condition = condition;
                result = (ConditionResultAsNumber + -0.5f) * 2;
            }
        }

        #region BooleanExtensions
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEqual(this bool2 @this, bool2 other)
        {
            return @this.x == other.x && @this.y == other.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AllTrue(this bool2 @this)
        {
            return @this.x && @this.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Any(this bool2 @this)
        {
            return @this.x || @this.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool None(this bool2 @this)
        {
            return !Any(@this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEqual(this bool3 @this, bool3 other)
        {
            return @this.x == other.x && @this.y == other.y && @this.z == other.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AllTrue(this bool3 @this)
        {
            return math.all(@this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Any(this bool3 @this)
        {
            return math.any(@this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool None(this bool3 @this)
        {
            return !Any(@this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEqual(this bool4 @this, bool4 other)
        {
            return @this.x == other.x && @this.y == other.y && @this.z == other.z && @this.w == other.w;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AllTrue(this bool4 @this)
        {
            return @this.x && @this.y && @this.z && @this.w;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Any(this bool4 @this)
        {
            return @this.x || @this.y || @this.z || @this.w;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool None(this bool4 @this)
        {
            return !Any(@this);
        }
        #endregion BooleanExtensions

        /// <summary>
        /// can be used to reproduce a local rotation ie transform parent/child rotation 
        /// see <paramref name="localEulerAngle"/>
        /// </summary>
        /// <param name="localEulerAngle">expected to be in radians</param>
        /// <param name="rotationReference">the rotation added with the angle</param>
        /// <returns></returns>
        public static quaternion RotateAngleRelativeToSource(in float3 localEulerAngle, in quaternion rotationReference)
        {
            return CalculateRotationRelativeToSource(rotationReference, quaternion.Euler(localEulerAngle));
        }

        public static quaternion CalculateRotationRelativeToSource(in quaternion rotationReference, in quaternion offsetRotation)
        {
            return math.mul(rotationReference, offsetRotation);
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlaps(this in AABB aabb, in AABB other)
        {
            return math.all(aabb.Max >= other.Min & aabb.Min <= other.Max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlaps(in float3 min, in float3 max, in float3 otherMin, in float3 otherMax)
        {
            return math.all(max >= otherMin & min <= otherMax);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Overlaps(this in Aabb aabb, in AABB other)
        {
            return Overlaps(in aabb.Min, in aabb.Max, other.Min, other.Max);
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RoundAsUIntWithFloatingPointAccuracy(this float value, uint accuracy = 100)
        {
            float f = value * accuracy;
            return RoundAsUInt(f) / accuracy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RoundAsUIntWithFloatingPointAccuracy(this double value, uint accuracy = 100)
        {
            return RoundAsUInt(value * accuracy) / accuracy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint3 RoundAsUInt(this float3 value, float roundValue = 0.5f)
        {
            return (uint3)(value + roundValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint3 RoundAsUInt(this double3 value, float roundValue = 0.5f)
        {
            return (uint3)(value + roundValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint3 RoundAsUIntWithFloatingPointAccuracy(this float3 value, uint accuracy = 100)
        {
            return RoundAsUInt(value * accuracy) / accuracy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint3 RoundAsUIntWithFloatingPointAccuracy(this double3 value, uint accuracy = 100)
        {
            return RoundAsUInt(value * accuracy) / accuracy;
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RoundAsUInt(this float value, float roundValue = 0.5f)
        {
            return (uint)(value + roundValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RoundAsUInt(this double value, float roundValue = 0.5f)
        {
            return (uint)(value + roundValue);
        }
    
    }
}