using UnityEditor;
using UnityEngine;

[CustomEditor(typeof (OnlineMapsGUITextureControl))]
public class OnlineMapsGUITextureControlEditor : Editor
{
    public override void OnInspectorGUI()
    {
        OnlineMapsControlBase control = target as OnlineMapsControlBase;
        OnlineMapsControlBaseEditor.CheckMultipleInstances(control);

        OnlineMaps api = OnlineMapsControlBaseEditor.GetOnlineMaps(control);
        OnlineMapsControlBaseEditor.CheckTarget(api, OnlineMapsTarget.texture);

        base.OnInspectorGUI();
    } 
}