#if SHAPES_URP && URP_EXISTS
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Shapes;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Profiling;
using Unity.Transforms;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Drboum.Utilities.Rendering
{
    struct CubeBakedRenderingData
    {
        public CubeBakedData CubeBakedData;
        public Color DrawColor;
        public float LineThickness;
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public partial class RenderingWireCubeSystem : SystemBase
    {
        private EntityQuery _drawCameraSettingsQuery;
        private NativeList<CubeBakedRenderingData> _batchedDrawingData;

        protected override void OnCreate()
        {
            base.OnCreate();
            OnCreate(ref CheckedStateRef);
        }

        public void OnCreate(ref SystemState state)
        {
            _drawCameraSettingsQuery = SystemAPI.QueryBuilder()
                .WithAll<RenderPropsCameraSetting>()
                .WithOptions(EntityQueryOptions.IncludeSystems)
                .Build();
            state.RequireForUpdate(_drawCameraSettingsQuery);

            if ( _drawCameraSettingsQuery.IsEmpty )
            {
                var renderPropsCameraSetting = new RenderPropsCameraSetting {
                    Camera = Camera.main
                };
                SetCameraFromSceneView(renderPropsCameraSetting);
                state.EntityManager.AddComponentObject(state.SystemHandle, renderPropsCameraSetting);
            }
            _batchedDrawingData = new(100, Allocator.Persistent);
        }

        [Conditional("UNITY_EDITOR")]
        private static void SetCameraFromSceneView(RenderPropsCameraSetting renderPropsCameraSetting)
        {
#if UNITY_EDITOR
            if ( !Application.isPlaying )
            {
                renderPropsCameraSetting.Camera = SceneView.lastActiveSceneView?.camera;
            }
#endif
        }

        protected override void OnUpdate()
        {
            OnUpdate(ref CheckedStateRef);
        }

        public void OnUpdate(ref SystemState state)
        {
            _batchedDrawingData.Clear();

            var allrenderingCubeDataQuery = SystemAPI.QueryBuilder()
                .WithAll<RenderWireCubeData>()
                .WithOptions(EntityQueryOptions.IncludeSystems)
                .Build();
            new BakeAndBatchForRenderingJob {
                BatchedDrawingData = _batchedDrawingData,
                RenderingDataHandle = SystemAPI.GetComponentTypeHandle<RenderWireCubeData>(true),
                LocalToWorldHandle = SystemAPI.GetComponentTypeHandle<LocalToWorld>(true),
            }.Run(allrenderingCubeDataQuery);
        }

        [BurstCompile]
        unsafe struct BakeAndBatchForRenderingJob : IJobChunk
        {
            private static readonly ProfilerMarker ExecuteMarker = new("BakeAndBatchForRenderingJobMarker");

            public NativeList<CubeBakedRenderingData> BatchedDrawingData;
            [ReadOnly] public ComponentTypeHandle<RenderWireCubeData> RenderingDataHandle;
            [ReadOnly, NativeDisableContainerSafetyRestriction] public ComponentTypeHandle<LocalToWorld> LocalToWorldHandle;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                ExecuteMarker.Begin();
                var renderWireCubes = chunk.GetRequiredComponentDataPtrROAsT(ref RenderingDataHandle);
                if ( chunk.Has<LocalToWorld>() )
                {
                    var localToWorldTransforms = chunk.GetRequiredComponentDataPtrROAsT(ref LocalToWorldHandle);

                    for ( int entityIndexInChunk = 0; entityIndexInChunk < chunk.Count; entityIndexInChunk++ )
                    {
                        var renderWireCubeData = renderWireCubes[entityIndexInChunk];
                        var localToWorld = localToWorldTransforms[entityIndexInChunk];
                        var cubeBakedRenderingData = ToCubeBakedRenderingData(renderWireCubeData);
                        cubeBakedRenderingData.CubeBakedData = new CubeBakedData(renderWireCubeData.CenterPosition, renderWireCubeData.HalfSize, localToWorld.Rotation);
                        BatchedDrawingData.Add(in cubeBakedRenderingData);
                    }
                }
                else
                {
                    for ( int entityIndexInChunk = 0; entityIndexInChunk < chunk.Count; entityIndexInChunk++ )
                    {
                        var renderWireCubeData = renderWireCubes[entityIndexInChunk];
                        var cubeBakedRenderingData = ToCubeBakedRenderingData(renderWireCubeData);
                        cubeBakedRenderingData.CubeBakedData = new CubeBakedData(renderWireCubeData.CenterPosition, renderWireCubeData.HalfSize);
                        BatchedDrawingData.Add(in cubeBakedRenderingData);
                    }
                }

                ExecuteMarker.End();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static CubeBakedRenderingData ToCubeBakedRenderingData(RenderWireCubeData renderWireCubeData)
        {
            return new CubeBakedRenderingData {
                LineThickness = renderWireCubeData.LineThickness,
                DrawColor = renderWireCubeData.DrawColor
            };
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnDestroy(ref CheckedStateRef);
        }

        public void OnDestroy(ref SystemState state)
        {
            _batchedDrawingData.Dispose();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            OnStartRunning(ref CheckedStateRef);
        }

        public void OnStartRunning(ref SystemState state)
        {
            UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += DrawShapesSRP;
        }

        void OnCameraPreRender(Camera cam)
        {
            if ( !_batchedDrawingData.IsCreated || _batchedDrawingData.IsEmpty )
                return;

            switch ( cam.cameraType )
            {
                case CameraType.Preview:
                case CameraType.Reflection:
                    return; // Don't render in preview windows or in reflection probes in case we run this script in the editor
            }
            // if( useCullingMasks && ( cam.cullingMask & ( 1 << gameObject.layer ) ) == 0 )
            //     return; // scene & game view cameras should respect culling layer settings if you tell them to
            Draw.PushMatrix();
            Draw.Matrix = Matrix4x4.identity;
            using ( Shapes.Draw.Command(cam) )
            {
                for ( var index = 0; index < _batchedDrawingData.Length; index++ )
                {
                    ref var cubeBakedRenderingData = ref _batchedDrawingData.ElementAt(index);
                    WiredCubeDrawer.DrawWiredCube(in cubeBakedRenderingData.CubeBakedData, cubeBakedRenderingData.LineThickness, cubeBakedRenderingData.DrawColor);
                }
            }
            Draw.PopMatrix();
        }

        private void DrawShapesSRP(UnityEngine.Rendering.ScriptableRenderContext ctx, Camera cam)
        {
            OnCameraPreRender(cam);
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            OnStopRunning(ref CheckedStateRef);
        }

        public void OnStopRunning(ref SystemState state)
        {
            UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= DrawShapesSRP;
        }
    }
}
#endif