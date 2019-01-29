using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioBootstrap : MonoBehaviour
{
    private void Awake(){
        if(AudioPlayer.Instance != null){
            return;
        }

        GameObject go = new GameObject("AudioEngine");
        go.AddComponent<AudioPlayer>();
    }
}
