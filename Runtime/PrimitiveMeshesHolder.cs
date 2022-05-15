using UnityEngine;
using UnityEngine.Rendering;
namespace Drboum.Utilities.Runtime {
    public class PrimitiveMeshesHolder : IDrawMeshes {
        private const         int               PRIMITIVE_MESH_COUNT        = (int)PrimitiveType.Quad + 1;
        internal const         ShadowCastingMode DEFAULT_SHADOW_CASTING_MODE = ShadowCastingMode.Off;
        internal const         LightProbeUsage   DEFAULT_LIGHT_PROBE_USAGE   = LightProbeUsage.Off;
        internal const         int               DEFAULT_RENDERING_LAYER     = 31;
        public static readonly int               LitColorPropertyId          = Shader.PropertyToID("_BaseColor");

        private MeshFilter[]          _meshFilters;
        private MeshRenderer[]        _meshRenderers;
        private MaterialPropertyBlock _materialPropertyBlock;
        public void CreatePrimitiveMeshes()
        {
            _meshFilters   = new MeshFilter[PRIMITIVE_MESH_COUNT];
            _meshRenderers = new MeshRenderer[PRIMITIVE_MESH_COUNT];
            for ( var i = 0; i < PRIMITIVE_MESH_COUNT; i++ ) {

                var displayMeshGobj = GameObject.CreatePrimitive((PrimitiveType)i);
                displayMeshGobj.SetActive(false);
                Object.DontDestroyOnLoad(displayMeshGobj);

                _meshFilters[i]   = displayMeshGobj.GetComponent<MeshFilter>();
                _meshRenderers[i] = displayMeshGobj.GetComponent<MeshRenderer>();

                _meshRenderers[i].material.enableInstancing = true;
                displayMeshGobj.RemoveComponentIfExists<Collider>();
            }
            _materialPropertyBlock = new MaterialPropertyBlock();
        }
        public void DrawMesh(PrimitiveType meshType, Vector3 position, Color color,
            Camera                         camera         = null,
            bool                           useLightProbes = false, bool receiveShadows = false, bool castShadows = false, int layer = DEFAULT_RENDERING_LAYER)
        {
            DrawMesh(meshType, position, Quaternion.identity, Vector3.one, color, camera, useLightProbes, receiveShadows, castShadows, layer);

        }
        public void DrawMesh(PrimitiveType meshType, Vector3 position, Quaternion rotation, Vector3 scale, Color color,
            Camera                         camera         = null,
            bool                           useLightProbes = false, bool receiveShadows = false, bool castShadows = false, int layer = DEFAULT_RENDERING_LAYER)
        {
            _materialPropertyBlock.SetColor(LitColorPropertyId, color);
            DrawMesh(meshType, position, rotation, scale, _materialPropertyBlock, camera, useLightProbes, receiveShadows, castShadows, layer);

        }
        public void DrawMesh(PrimitiveType meshType, Vector3 position, Quaternion rotation, Vector3 scale, MaterialPropertyBlock materialPropertyBlock,
            Camera                         camera         = null,
            bool                           useLightProbes = false, bool receiveShadows = false, bool castShadows = false, int layer = DEFAULT_RENDERING_LAYER)
        {
            var index = (int)meshType;
            Graphics.DrawMesh(_meshFilters[index].mesh, Matrix4x4.TRS(position, rotation, scale), _meshRenderers[index].material, DEFAULT_RENDERING_LAYER, camera, 0,
                materialPropertyBlock, castShadows,
                receiveShadows,
                useLightProbes);

        }
        public void DrawMeshBatch(PrimitiveType meshType, Matrix4x4[] matrices, int count, Color color,
            ShadowCastingMode                   castShadows           = DEFAULT_SHADOW_CASTING_MODE,
            bool                                receiveShadows        = false,
            int                                 layer                 = DEFAULT_RENDERING_LAYER,
            Camera                              camera                = null,
            LightProbeUsage                     lightProbeUsage       = DEFAULT_LIGHT_PROBE_USAGE,
            LightProbeProxyVolume               lightProbeProxyVolume = null)
        {
            _materialPropertyBlock.SetColor(LitColorPropertyId, color);
            DrawMeshBatch(meshType, matrices, count, _materialPropertyBlock,
                castShadows,
                receiveShadows,
                layer,
                camera,
                lightProbeUsage
            );
        }
        public void DrawMeshBatch(PrimitiveType meshType, Matrix4x4[] matrices, int count, MaterialPropertyBlock materialPropertyBlock,
            ShadowCastingMode                   castShadows           = DEFAULT_SHADOW_CASTING_MODE,
            bool                                receiveShadows        = false,
            int                                 layer                 = DEFAULT_RENDERING_LAYER,
            Camera                              camera                = null,
            LightProbeUsage                     lightProbeUsage       = DEFAULT_LIGHT_PROBE_USAGE,
            LightProbeProxyVolume               lightProbeProxyVolume = null
        )
        {
            var index = (int)meshType;
            Graphics.DrawMeshInstanced(_meshFilters[index].mesh, 0, _meshRenderers[index].material, matrices, count, materialPropertyBlock,
                castShadows,
                receiveShadows,
                layer,
                camera,
                lightProbeUsage,
                lightProbeProxyVolume
            );

        }
    }
}