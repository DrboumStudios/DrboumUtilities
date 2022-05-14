using UnityEngine;
namespace DrboumLibrary {
    public static class CameraExtensions {
        public static Texture2D RenderTextureImage(this Camera camera)
        {
            // The Render Texture in RenderTexture.active is the one
            // that will be read by ReadPixels.
            var           currentRT           = RenderTexture.active;
            RenderTexture cameraTargetTexture = camera.targetTexture;
            RenderTexture.active = cameraTargetTexture;

            // Render the camera's view.
            camera.Render();

            // Make a new texture and read the active Render Texture into it.
            Texture2D image = new Texture2D(cameraTargetTexture.width, cameraTargetTexture.height);
            image.ReadPixels(new Rect(0, 0, cameraTargetTexture.width, cameraTargetTexture.height), 0, 0);
            image.Apply();

            // Replace the original active Render Texture.
            RenderTexture.active = currentRT;
            return image;
        }
    }
}