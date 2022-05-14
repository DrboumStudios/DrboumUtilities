using UnityEngine;
using UnityEngine.Rendering;
namespace DrboumLibrary {
    public interface IDrawMeshes {
        void DrawMesh(PrimitiveType meshType, Vector3 position, Color color,
            Camera                  camera         = null,
            bool                    useLightProbes = false, bool receiveShadows = false, bool castShadows = false, int layer = 31);
        void DrawMesh(PrimitiveType meshType, Vector3 position, Quaternion rotation, Vector3 scale, Color color,
            Camera                  camera         = null,
            bool                    useLightProbes = false, bool receiveShadows = false, bool castShadows = false, int layer = 31);
        void DrawMesh(PrimitiveType meshType, Vector3 position, Quaternion rotation, Vector3 scale, MaterialPropertyBlock materialPropertyBlock,
            Camera                  camera         = null,
            bool                    useLightProbes = false, bool receiveShadows = false, bool castShadows = false, int layer = 31);
        void DrawMeshBatch(PrimitiveType meshType, Matrix4x4[] matrices, int count, Color color,
            ShadowCastingMode             castShadows           = PrimitiveMeshesHolder.DEFAULT_SHADOW_CASTING_MODE,
            bool                          receiveShadows        = false,
            int                           layer                 = PrimitiveMeshesHolder.DEFAULT_RENDERING_LAYER,
            Camera                        camera                = null,
            LightProbeUsage               lightProbeUsage       = PrimitiveMeshesHolder.DEFAULT_LIGHT_PROBE_USAGE,
            LightProbeProxyVolume         lightProbeProxyVolume =null  );
        void DrawMeshBatch(PrimitiveType meshType, Matrix4x4[] matrices, int count, MaterialPropertyBlock materialPropertyBlock,
            ShadowCastingMode             castShadows           = PrimitiveMeshesHolder.DEFAULT_SHADOW_CASTING_MODE,
            bool                          receiveShadows        = false,
            int                           layer                 = PrimitiveMeshesHolder.DEFAULT_RENDERING_LAYER,
            Camera                        camera                = null,
            LightProbeUsage               lightProbeUsage       = PrimitiveMeshesHolder.DEFAULT_LIGHT_PROBE_USAGE,
            LightProbeProxyVolume         lightProbeProxyVolume =null 
        );
    }
}