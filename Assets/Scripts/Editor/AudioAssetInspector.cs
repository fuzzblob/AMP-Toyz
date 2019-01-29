using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioAsset))]
public class AudioAssetInspector : Editor
{
    AudioAsset asset;

    SerializedProperty Clips;
    SerializedProperty PlayMode;
    SerializedProperty Weights;

    SerializedProperty PitchBase;
    SerializedProperty PitchOffset;
    SerializedProperty VolumeBase;
    SerializedProperty VolumeOffset;

    void OnEnable(){
        asset = target as AudioAsset;

        Clips = serializedObject.FindProperty("Clips");
        PlayMode = serializedObject.FindProperty("PlayMode");
        Weights = serializedObject.FindProperty("Weights");
        
        PitchBase = serializedObject.FindProperty("PitchBase");
        PitchOffset = serializedObject.FindProperty("PitchOffset");
        VolumeBase = serializedObject.FindProperty("VolumeBase");
        VolumeOffset = serializedObject.FindProperty("VolumeOffset");

    }

    public override void OnInspectorGUI(){
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(Clips, true);
        if (EditorGUI.EndChangeCheck()) {
            MatchWeightsWithClips();
        }

        EditorGUILayout.PropertyField(PlayMode);

        if (asset.PlayMode == PlaylistMode.WeightedRandom){
            EditorGUILayout.PropertyField(Weights, true);
        }

        EditorGUILayout.PropertyField(PitchBase);
        EditorGUILayout.PropertyField(PitchOffset);
        EditorGUILayout.PropertyField(VolumeBase);
        EditorGUILayout.PropertyField(VolumeOffset);

        serializedObject.ApplyModifiedProperties();
    }

    private void MatchWeightsWithClips(){
        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();

        while (asset.Weights.Count > asset.Clips.Length) {
            asset.Weights.RemoveAt(asset.Weights.Count-1);
        }

        float autofillWeight = asset.Weights[asset.Weights.Count-1];
        while (asset.Weights.Count < asset.Clips.Length) {
            asset.Weights.Add(autofillWeight);
        }
    }
}
