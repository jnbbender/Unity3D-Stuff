using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace NastyDiaper
{
    public class ScreenShot : Singleton<ScreenShot>
    {
        [Range(0f, 2048f)]
        public int width = 600;
        [Range(0f, 2048)]
        public int height = 200;

        [Space(5)]
        public bool useCamera = true;
        [Header("Make sure the depth of this camera is lower than the in-game camera")]
        [SerializeField]
        [ShowIf("useCamera")]
        Camera shotCamera;

        [Space(10)]
        public bool debugShot = false;
        [SerializeField]
        [ShowIf("debugShot")]
        Image toImage;

        Texture2D screenShotTexture;

        [Button]
        [ExecuteInEditMode]
        public void Clear()
        {
            if (toImage)
            {
                toImage.sprite = null;
                toImage.enabled = false;
            }
        }

        [Button]
        [ExecuteInEditMode]
        public void TakeScreenShot()
        {
            if (useCamera)
            {
                // Setup my screenshot camera to point to the main game camera.
                shotCamera.transform.position = Camera.main.transform.position;
                shotCamera.transform.rotation = Camera.main.transform.rotation;

                shotCamera.enabled = true;

                // create the new RenderTexture and set it active
                RenderTexture rt = new RenderTexture(width, height, 16);
                RenderTexture.active = rt;

                // assign it to our screenshot camera, render it & shutoff the camera
                shotCamera.targetTexture = rt;
                shotCamera.Render();
                shotCamera.enabled = false;
            }

            // setup a Texture2D of the same size 
            screenShotTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            // ReadPixels will read from the current render
            screenShotTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenShotTexture.Apply();

            // if we decided to debug and pass in an image...
            if (toImage)
            {
                // resize the RectTransform to match our width & height
                RectTransform rectTransform = toImage.GetComponentInParent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(width, height);

                // create the new sprite and we're all good.
                var sprite = Sprite.Create(screenShotTexture, new Rect(0, 0, width, height), new Vector2(0, 0));
                toImage.sprite = sprite;
                if (debugShot && toImage)
                    toImage.enabled = true;
            }
        }

        public Texture2D GetTexture()
        {
            return screenShotTexture;
        }

        public Sprite GetSprite()
        {
            return toImage.sprite;
        }
    }
}
