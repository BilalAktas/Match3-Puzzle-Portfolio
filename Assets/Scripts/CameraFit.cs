using UnityEngine;

namespace Portfolio.Match3.Helpers
{
    
    /// <summary>
    /// Adjusts the camera's orthographic size to fit the target resolution aspect ratio.
    /// </summary>
    public class CameraFit : MonoBehaviour
    {
        private const float _TARGET_WIDTH = 1920f;
        private const float _TARGET_HEIGHT = 1080f;
        public float TargetOrthographicSize = 5f;

        private void Awake() => AdjustCameraSize();

        private void AdjustCameraSize()
        {
            var orthographicCamera = GetComponent<Camera>();
        
 
            var screenAspect = (float)Screen.width / Screen.height;
            var targetAspect = _TARGET_WIDTH / _TARGET_HEIGHT;


            if (screenAspect >= targetAspect)
                orthographicCamera.orthographicSize = TargetOrthographicSize;
            else
            {
                var differenceInSize = targetAspect / screenAspect;
                orthographicCamera.orthographicSize = TargetOrthographicSize * differenceInSize;
            }
        }
    }
}

