using System.Collections.Generic;
namespace Drboum.Utilities.Runtime.Animation
{
    public static class AnimatorLayerExt
    {
        public static void SyncLayerIndexes(List<AnimatorLayer> _animatorLayerBuffer, string[] _folders)
        {
            UnityObjectEditorHelper.FindAllAssetInstances(_animatorLayerBuffer, _folders);
            for ( var i = 0; i < _animatorLayerBuffer.Count; i++ )
            {
                AnimatorLayer animatorLayer = _animatorLayerBuffer[i];
                animatorLayer.SyncLayerIndex();
            }

        }
    }
}