using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioAsset))]
public class AudioAssetInspector : Editor
{
    private static Voice _inspectorVoice;

    AudioAsset asset;

    SerializedProperty Priority;
    SerializedProperty Clips;
    SerializedProperty Looping;
    SerializedProperty PlayMode;
    SerializedProperty Weights;

    SerializedProperty PitchBase;
    SerializedProperty PitchOffset;
    SerializedProperty VolumeBase;
    SerializedProperty VolumeOffset;

    SerializedProperty FadeTypeIn;
    SerializedProperty FadeTypeOut;
    SerializedProperty FadeTimeIn;
    SerializedProperty FadeTimeOut;

    SerializedProperty Attenuation;
    SerializedProperty MinimumDistance;
    SerializedProperty MaximumDistance;

    void OnEnable(){
        asset = target as AudioAsset;

        Priority = serializedObject.FindProperty("Priority");
        Clips = serializedObject.FindProperty("Clips");
        Looping = serializedObject.FindProperty("Looping");
        PlayMode = serializedObject.FindProperty("PlayMode");
        Weights = serializedObject.FindProperty("Weights");
        
        PitchBase = serializedObject.FindProperty("PitchBase");
        PitchOffset = serializedObject.FindProperty("PitchOffset");
        VolumeBase = serializedObject.FindProperty("VolumeBase");
        VolumeOffset = serializedObject.FindProperty("VolumeOffset");
        
        FadeTypeIn = serializedObject.FindProperty("FadeTypeIn");
        FadeTypeOut = serializedObject.FindProperty("FadeTypeOut");
        FadeTimeIn = serializedObject.FindProperty("FadeTimeIn");
        FadeTimeOut = serializedObject.FindProperty("FadeTimeOut");

        Attenuation = serializedObject.FindProperty("Attenuation");
        MinimumDistance = serializedObject.FindProperty("MinimumDistance");
        MaximumDistance = serializedObject.FindProperty("MaximumDistance");
    }

    public override void OnInspectorGUI(){
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(Clips, true);
        if (EditorGUI.EndChangeCheck()) {
            MatchWeightsWithClips();
        }

        EditorGUILayout.PropertyField(Priority);
        EditorGUILayout.PropertyField(Looping);

        EditorGUILayout.PropertyField(PlayMode);

        if (asset.PlayMode == PlaylistMode.WeightedRandom){
            EditorGUILayout.PropertyField(Weights, true);
        }

        EditorGUILayout.PropertyField(PitchBase);
        EditorGUILayout.PropertyField(PitchOffset);
        EditorGUILayout.PropertyField(VolumeBase);
        EditorGUILayout.PropertyField(VolumeOffset);

        EditorGUILayout.PropertyField(FadeTimeIn);
        EditorGUILayout.PropertyField(FadeTypeIn);        
        EditorGUILayout.PropertyField(FadeTimeOut);
        EditorGUILayout.PropertyField(FadeTypeOut);
        
        EditorGUILayout.PropertyField(Attenuation);
        EditorGUILayout.PropertyField(MinimumDistance);
        EditorGUILayout.PropertyField(MaximumDistance);
        
        serializedObject.ApplyModifiedProperties();

        if(GUILayout.Button("Play")){
            _inspectorVoice = asset.Play();
        }
        if(_inspectorVoice == null)
            GUI.enabled = false;
        if(GUILayout.Button("Stop")){
            AudioPlayer.Instance.Stop(_inspectorVoice);
        }
        GUI.enabled = true;
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
