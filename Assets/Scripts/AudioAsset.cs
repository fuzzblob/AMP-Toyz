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

public enum AttenuationMode {
    None = 0,
    Linear,
    Logarithmic
}

[CreateAssetMenu(fileName = "AudioAsset", menuName = "Audio/Create Asset")]
public class AudioAsset : ScriptableObject
{
    // Playlist
    public AudioClip[] Clips;
    
    // Playback
    public bool Looping = false;
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
    [Range(0f, 10f)]
    public float FadeTimeIn;
    public FadeType FadeTypeIn;
    [Range(0f, 10f)]
    public float FadeTimeOut;
    public FadeType FadeTypeOut;
    
    //Distance & Attenuation
    public AttenuationMode Attenuation = AttenuationMode.None;
    [Range(0f, 500f)]
    public float MinimumDistance = 1f;
    [Range(0f, 1000f)]
    public float MaximumDistance = 50f;

    public Voice Play(){
        return AudioPlayer.Instance.Play(this);
    }

    private void OnEnable() {
        // ensure first playback is 0 if playing in sequence
        LastIndex = -1;
    }
}
