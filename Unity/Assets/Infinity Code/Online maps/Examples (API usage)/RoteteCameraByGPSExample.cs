/*     INFINITY CODE 2013-2015      */
/*   http://www.infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    [AddComponentMenu("")]
    public class RoteteCameraByGPSExample : MonoBehaviour
    {
        private void Start()
        {
            OnlineMapsLocationService.instance.OnCompassChanged += OnCompassChanged;
        }

        private void OnCompassChanged(float f)
        {
            OnlineMapsTileSetControl.instance.cameraRotation.y = f * 360;
        }
    }
}