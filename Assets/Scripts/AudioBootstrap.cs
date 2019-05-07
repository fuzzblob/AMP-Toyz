using UnityEngine;

public sealed class AudioBootstrap : MonoBehaviour {
    private void Awake(){
        AudioPlayer.SafeInstance();
    }
}
