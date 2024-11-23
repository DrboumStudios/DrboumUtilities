#if HAS_SHAPE_ASSET
using System;
using System.Runtime.CompilerServices;
using Shapes;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Drboum.Utilities.Rendering
{
    [ExecuteAlways]
    public class WiredCubeDrawer : ImmediateModeShapeDrawer
    {
        [SerializeField] private Color _shapeColor = Color.red * new Color(1, 1, 1, 0.5f);
        [SerializeField] private float lineThickness = 1f;

        internal bool _debugDisplay = false;
        private UObjectWrapper<Transform> _transform;
        private AABB _bounds;

        private Transform Transform {
            get {
                if ( !_transform.HasValue )
                {
                    _transform.Value = transform;
                }
                return _transform.Value;
            }
        }

        public float LineThickness {
            get => lineThickness;
            set => lineThickness = value;
        }

        public void SetDrawingData(AABB bounds)
        {
            _bounds = bounds;
        }

        public override void DrawShapes(Camera cam)
        {
            if ( !enabled || !_debugDisplay )
                return;
        
            DrawWiredCubeCommand(cam, Transform.localToWorldMatrix, lineThickness, _shapeColor, _bounds.Center, _bounds.Size);
        }

        public static void DrawWiredCubeCommand(Camera cam, Matrix4x4 transformLocalToWorldMatrix, float thickness, Color shapeColor, Vector3 spawnBoundsCenter, float3 size)
        {
            using ( Draw.Command(cam) )
            {
                // set up static parameters. these are used for all following Draw.Line calls
                SetupDrawSettings(thickness, transformLocalToWorldMatrix);
                // draw lines
                DrawWiredCube(spawnBoundsCenter, size * .5f, thickness, shapeColor);
            }
        }

        public static void SetupDrawSettings(float thickness, Matrix4x4 localMatrix)
        {
            Draw.LineGeometry = LineGeometry.Volumetric3D;
            Draw.ThicknessSpace = ThicknessSpace.Meters;
            Draw.Thickness = thickness;
            // set static parameter to draw in the local space of this object
            Draw.Matrix = localMatrix;
        }

        public static void DrawWiredCube(Vector3 centerPosition, Vector3 halfExtents, float thickness, Color shapeColor)
        {
            var bakedwireCube = new CubeBakedData(centerPosition, halfExtents);
            DrawWiredCube(in bakedwireCube, thickness, shapeColor);
        }

        public static void DrawWiredCube(float3 centerPosition, float3 halfExtents, quaternion transformRotation, float thickness, Color shapeColor)
        {
            var bakedwireCube = new CubeBakedData(centerPosition, halfExtents, transformRotation);
            DrawWiredCube(in bakedwireCube, thickness, shapeColor);
        }

        public static void DrawWiredCube(in CubeBakedData cubeBakedData, float thickness, Color shapeColor)
        {
            //draw lines up
            DrawWireRectangle(cubeBakedData.pointAUp, cubeBakedData.pointBUp, cubeBakedData.pointCUp, cubeBakedData.pointDUp, thickness, shapeColor);

            //draw lines bottom
            DrawWireRectangle(cubeBakedData.pointADown, cubeBakedData.pointBDown, cubeBakedData.pointCDown, cubeBakedData.pointDDown, thickness, shapeColor);

            //joint with top and bottom
            Draw.Line(cubeBakedData.pointAUp, cubeBakedData.pointADown, thickness: thickness, color: shapeColor);
            Draw.Line(cubeBakedData.pointBUp, cubeBakedData.pointBDown, thickness: thickness, color: shapeColor);
            Draw.Line(cubeBakedData.pointCUp, cubeBakedData.pointCDown, thickness: thickness, color: shapeColor);
            Draw.Line(cubeBakedData.pointDUp, cubeBakedData.pointDDown, thickness: thickness, color: shapeColor);
        }

        private static void DrawWireRectangle(in float3 pointA, in float3 pointB, in float3 pointC, in float3 pointD, float thickness, in Color shapeColor)
        {
            Draw.Line(pointA, pointB, thickness: thickness, color: shapeColor);
            Draw.Line(pointA, pointC, thickness: thickness, color: shapeColor);
            Draw.Line(pointD, pointC, thickness: thickness, color: shapeColor);
            Draw.Line(pointD, pointB, thickness: thickness, color: shapeColor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 GetWorldPointPosition(in float3 centerPosition, in float3 relativePoint)
        {
            return centerPosition + relativePoint;
        }

        private static void GetUpPointAndDownPoints(Vector3 halfExtents, float3 point, in quaternion transformRotation, out float3 pointUp, out float3 pointDown)
        {
            pointUp = math.mul(transformRotation, new float3(point.x, halfExtents.y, point.z));
            pointDown = math.mul(transformRotation, new float3(point.x, -halfExtents.y, point.z));
        }
    }

    public struct CubeBakedData
    {
        public float3 pointAUp;
        public float3 pointADown;
        public float3 pointBUp;
        public float3 pointBDown;
        public float3 pointCUp;
        public float3 pointCDown;
        public float3 pointDUp;
        public float3 pointDDown;

        public CubeBakedData(float3 centerPosition, float3 halfExtents)
        {
            GetRelativePoints(halfExtents,
                out pointAUp,
                out pointADown,
                out pointBUp,
                out pointBDown,
                out pointCUp,
                out pointCDown,
                out pointDUp,
                out pointDDown
            );
            ConvertToWorldSpacePoint(in centerPosition, ref pointAUp);
            ConvertToWorldSpacePoint(in centerPosition, ref pointADown);
            ConvertToWorldSpacePoint(in centerPosition, ref pointBUp);
            ConvertToWorldSpacePoint(in centerPosition, ref pointBDown);
            ConvertToWorldSpacePoint(in centerPosition, ref pointCUp);
            ConvertToWorldSpacePoint(in centerPosition, ref pointCDown);
            ConvertToWorldSpacePoint(in centerPosition, ref pointDUp);
            ConvertToWorldSpacePoint(in centerPosition, ref pointDDown);
        }

        public CubeBakedData(float3 centerPosition, float3 halfExtents, quaternion transformRotation)
        {
            GetRelativePoints(halfExtents,
                out pointAUp,
                out pointADown,
                out pointBUp,
                out pointBDown,
                out pointCUp,
                out pointCDown,
                out pointDUp,
                out pointDDown
            );

            pointAUp = centerPosition + math.mul(transformRotation, pointAUp);
            pointADown = centerPosition + math.mul(transformRotation, pointADown);
            pointBUp = centerPosition + math.mul(transformRotation, pointBUp);
            pointBDown = centerPosition + math.mul(transformRotation, pointBDown);
            pointCUp = centerPosition + math.mul(transformRotation, pointCUp);
            pointCDown = centerPosition + math.mul(transformRotation, pointCDown);
            pointDUp = centerPosition + math.mul(transformRotation, pointDUp);
            pointDDown = centerPosition + math.mul(transformRotation, pointDDown);
        }

        private static void GetRelativePoints(float3 halfExtents, out float3 pointAUp, out float3 pointADown, out float3 pointBUp, out float3 pointBDown, out float3 pointCUp, out float3 pointCDown, out float3 pointDUp, out float3 pointDDown)
        {
            var pointA = new float3(-halfExtents.x, 0, -halfExtents.z);
            var pointB = new float3(halfExtents.x, 0, -halfExtents.z);
            var pointC = new float3(-halfExtents.x, 0, halfExtents.z);
            var pointD = new float3(halfExtents.x, 0, halfExtents.z);
            pointAUp = new float3(pointA.x, (halfExtents).y, pointA.z);
            pointADown = new float3(pointA.x, -(halfExtents).y, pointA.z);
            pointBUp = new float3(pointB.x, (halfExtents).y, pointB.z);
            pointBDown = new float3(pointB.x, -(halfExtents).y, pointB.z);
            pointCUp = new float3(pointC.x, (halfExtents).y, pointC.z);
            pointCDown = new float3(pointC.x, -(halfExtents).y, pointC.z);
            pointDUp = new float3(pointD.x, (halfExtents).y, pointD.z);
            pointDDown = new float3(pointD.x, -(halfExtents).y, pointD.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ConvertToWorldSpacePoint(in float3 centerPosition, ref float3 point)
        {
            point += centerPosition;
        }
    }

    public struct UObjectWrapper<TObject>
        where TObject : Object
    {
        private TObject _value;
        public bool HasValue {
            get;
            private set;
        }

        public TObject Value {
            readonly get => _value;
            set {
                HasValue = value;
                _value = value;
            }
        }

        public static implicit operator TObject(UObjectWrapper<TObject> wrapper)
        {
            return wrapper._value;
        }
    }

    public struct DrawCommandWrapper : IDisposable
    {
        public DrawCommand DrawCommand;

        public void Dispose()
        {
            DrawCommand.Dispose();
        }

        public static DrawCommandWrapper Create(Camera cam, ThicknessSpace thicknessSpace = ThicknessSpace.Meters, LineGeometry lineGeometry = LineGeometry.Volumetric3D)
        {
            return Create(cam, Matrix4x4.identity, thicknessSpace, lineGeometry);
        }

        public static DrawCommandWrapper Create(Camera cam, Matrix4x4 localMatrix, ThicknessSpace thicknessSpace = ThicknessSpace.Meters, LineGeometry lineGeometry = LineGeometry.Volumetric3D)
        {
            var drawCommandWrapper = new DrawCommandWrapper { DrawCommand = Shapes.Draw.Command(cam) };

            Draw.LineGeometry = lineGeometry;
            Draw.ThicknessSpace = thicknessSpace;
            // set static parameter to draw in the local space of this object
            Draw.Matrix = localMatrix;

            return drawCommandWrapper;
        }
    }
#endif
}