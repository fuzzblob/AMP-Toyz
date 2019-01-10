using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioPlayer))]
public class AudioPlayerInspector : Editor
{
    AudioPlayer player;

    public override void OnInspectorGUI(){
        player = target as AudioPlayer;
        base.OnInspectorGUI();

        GUI.enabled = Application.isPlaying;

        if(GUILayout.Button("Play")){
            player.Sound.Play();
        }
    }
}
