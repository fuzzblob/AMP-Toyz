using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This enum describes how a collection of sounds should be played back
/// </summary>
public enum PlaylistMode{
    Sequence = 0,
    Random = 1,
    Shuffle = 2,
    WeightedRandom = 3
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
    public Queue<int> ShuffleQueue;
    [Tooltip("Chance to play associated with each AudioClip")]
    [Range(0f, 100f)]
    public List<float> Weights;

    // Audio
    [Range(-24, 24)]
    public float PitchBase;
    [Range(0, 24)]
    public float PitchOffset;
    [Range(-80, 0)]
    public float VolumeBase;
    [Range(0, 20)]
    public float VolumeOffset;

    // Fade data
    public FadeType FadeType;
    public float FadeInPointMs;
    public float FadeOutPointMs;

    public void Play(){
        AudioPlayer.Instance.Play(this);
    }

    private void OnEnable() {
        // ensure first playback is 0 if playing in sequence
        LastIndex = -1;
    }
}
