/*     INFINITY CODE 2013-2015      */
/*   http://www.infinity-code.com   */

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OnlineMapsIGUITextureControl))]
public class OnlineMapsIGUITextureControlEditor : Editor
{
    public override void OnInspectorGUI()
    {
        OnlineMapsControlBase control = target as OnlineMapsControlBase;
        OnlineMapsControlBaseEditor.CheckMultipleInstances(control);

        OnlineMaps api = OnlineMapsControlBaseEditor.GetOnlineMaps(control);
        OnlineMapsControlBaseEditor.CheckTarget(api, OnlineMapsTarget.texture);

#if !IGUI
        if (GUILayout.Button("Enable iGUI"))
        {
            string currentDefinitions =
            PlayerSettings.GetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup);
            if (currentDefinitions != "") currentDefinitions += ";";
            currentDefinitions += "IGUI";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currentDefinitions);
        }
#else
        base.OnInspectorGUI();
#endif
    }
}