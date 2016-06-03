/*     INFINITY CODE 2013-2015      */
/*   http://www.infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    [AddComponentMenu("")]
    public class TooltipWithoutBackgroundExample : MonoBehaviour
    {

        private void Start()
        {
            // Subscribe to the event preparation of tooltip style.
            OnlineMaps.instance.OnPrepareTooltipStyle += OnPrepareTooltipStyle;
        }

        private void OnPrepareTooltipStyle(ref GUIStyle style)
        {
            // Hide background.
            style.normal.background = null;
        }
    }
}