/*     INFINITY CODE 2013-2015      */
/*   http://www.infinity-code.com   */

using UnityEngine;

namespace InfinityCode.OnlineMapsExamples
{
    [AddComponentMenu("")]
    public class FindAutocompleteExample : MonoBehaviour
    {
        private void Start()
        {
            OnlineMapsFindAutocomplete.Find(
                "Los ang",
                "" // <----------------------------- Google API Key
                ).OnComplete += OnComplete;
        }

        private void OnComplete(string s)
        {
            OnlineMapsFindAutocompleteResult[] results = OnlineMapsFindAutocomplete.GetResults(s);
            if (results == null)
            {
                Debug.Log("Error");
                Debug.Log(s);
                return;
            }
            foreach (OnlineMapsFindAutocompleteResult result in results)
            {
                Debug.Log(result.description);
            }
        }
    }
}