/*     INFINITY CODE 2013-2015      */
/*   http://www.infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    [AddComponentMenu("")]
    public class DragMarkerByLongPressExample : MonoBehaviour
    {
        private void Start()
        {
            // Create a new marker.
            OnlineMapsMarker marker = OnlineMaps.instance.AddMarker(Vector2.zero, "My Marker");

            // Subscribe to OnLongPress event.
            marker.OnLongPress += OnMarkerLongPress;
        }

        private void OnMarkerLongPress(OnlineMapsMarkerBase marker)
        {
            // Starts moving the marker.
            OnlineMapsControlBase.instance.dragMarker = marker;
            OnlineMapsControlBase.instance.isMapDrag = false;
        }
    }
}