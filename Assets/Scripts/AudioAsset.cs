using UnityEngine;

public enum PlaylistMode{
    Sequence = 0,
    Random = 1,
    Shuffle = 2,
    // TODO: implement
    // Weighted = 3,
}

[CreateAssetMenu(fileName = "AudioAsset", menuName = "Audio/Create Asset")]
public class AudioAsset : ScriptableObject
{
    // Playlist
    public AudioClip[] Clips;
    
    // Playback
    public PlaylistMode PlayMode;
    [System.NonSerialized]
    public int LastIndex;
    [System.NonSerialized]
    public System.Collections.Generic.Queue<int> ShuffleQueue;

    // Audio
    [Range(-24, 24)]
    public float PitchBase;
    [Range(0, 24)]
    public float PitchOffset;
    [Range(-80, 0)]
    public float VolumeBase;
    [Range(0, 20)]
    public float VolumeOffset;

    public void Play(){
        AudioPlayer.Instance.Play(this);
    }

    private void OnEnable() {
        // ensure first playback is 0 if playing in sequence
        LastIndex = -1;
    }
}
