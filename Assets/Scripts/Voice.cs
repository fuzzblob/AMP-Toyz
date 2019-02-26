using UnityEngine;

public class Voice {
    public AudioSource Source;
    public AudioAsset Asset;
    public Vector3 Position;
    public AudioClip Clip;
    public float PlaybackPosition;

    public float Volume;
    public float Pitch;
    public FadeBehaviour Fader;

    public AttenuationMode Attenuation;
    public float MinDistance;
    public float MaxDistance;
}