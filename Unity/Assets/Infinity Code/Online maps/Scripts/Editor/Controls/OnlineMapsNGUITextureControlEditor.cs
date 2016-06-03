/*     INFINITY CODE 2013-2015      */
/*   http://www.infinity-code.com   */

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OnlineMapsNGUITextureControl))]
public class OnlineMapsNGUITextureControlEditor : Editor
{
    public override void OnInspectorGUI()
    {
        OnlineMapsControlBase control = target as OnlineMapsControlBase;
        OnlineMapsControlBaseEditor.CheckMultipleInstances(control);

        OnlineMaps api = OnlineMapsControlBaseEditor.GetOnlineMaps(control);
        OnlineMapsControlBaseEditor.CheckTarget(api, OnlineMapsTarget.texture);

#if !NGUI
        if (GUILayout.Button("Enable NGUI"))
        {
            string currentDefinitions =
            PlayerSettings.GetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup);
            if (currentDefinitions != "") currentDefinitions += ";";
            currentDefinitions += "NGUI";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currentDefinitions);
        }
#else
        base.OnInspectorGUI();
#endif
    }
}